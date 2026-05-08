// <copyright file="FileRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Globalization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class FileRepository(IConfiguration _config) : IFileRepository
    {
        private readonly string fileUploadPath = _config["Data:FileUploadPath"] ?? throw new InternalErrorException("Could not load file upload path.");
        private readonly bool googleDriveEnabled = _config.GetValue<bool>("GoogleDrive:Enabled");
        private readonly string? googleDriveRootFolderId = _config["GoogleDrive:RootFolderId"];
        private readonly string? googleDriveServiceAccountKeyPath = _config["GoogleDrive:ServiceAccountKeyPath"];

        /// <inheritdoc/>
        public async Task<byte[]> GetByPath(string filePath)
        {
            if (!filePath.StartsWith(this.fileUploadPath))
            {
                throw new InternalErrorException("Accessing unallowed path");
            }

            if (!File.Exists(filePath))
            {
                throw new NotFoundException("Could not find receipt.");
            }

            return await File.ReadAllBytesAsync(filePath);
        }

        /// <inheritdoc/>
        public async Task<string> SaveFile(IFormFile file, string name)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidFileException("No file uploaded");
            }

            // Get file name to prevent code/file access injections
            var safeFilePath = Path.GetFileName(name);

            // Get extension safely
            var fileEnding = Path.GetExtension(file.FileName);

            // Forge safe file name
            var safeFileName = $"{safeFilePath}{fileEnding}";

            var filePath = Path.Combine(this.fileUploadPath, safeFileName);

            Directory.CreateDirectory(this.fileUploadPath);

            await using (var stream = File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            if (this.googleDriveEnabled)
            {
                await this.UploadToGoogleDriveAsync(filePath, safeFileName);
            }

            return filePath;
        }

        private static string EscapeDriveQueryValue(string value)
        {
            return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal);
        }

        private static string GetContentType(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream",
            };
        }

        private async Task UploadToGoogleDriveAsync(string localFilePath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(this.googleDriveRootFolderId))
            {
                throw new InternalErrorException("Google Drive root folder id is not configured.");
            }

            if (string.IsNullOrWhiteSpace(this.googleDriveServiceAccountKeyPath))
            {
                throw new InternalErrorException("Google Drive service account key path is not configured.");
            }

            if (!File.Exists(this.googleDriveServiceAccountKeyPath))
            {
                throw new InternalErrorException("Google Drive service account key file could not be found.");
            }

            using var driveService = this.CreateDriveService();
            var now = DateTime.UtcNow;
            var yearFolderId = await this.GetOrCreateFolderAsync(driveService, now.Year.ToString(CultureInfo.InvariantCulture), this.googleDriveRootFolderId);
            var monthFolderId = await this.GetOrCreateFolderAsync(driveService, now.ToString("MMMM", CultureInfo.InvariantCulture), yearFolderId);

            await using var stream = File.OpenRead(localFilePath);
            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = fileName,
                Parents = [monthFolderId],
            };

            var request = driveService.Files.Create(fileMetadata, stream, GetContentType(fileName));
            request.Fields = "id";

            var result = await request.UploadAsync();

            if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                throw new InternalErrorException("Uploading invoice to Google Drive failed.");
            }
        }

        private DriveService CreateDriveService()
        {
            var credential = CredentialFactory
                .FromFile<ServiceAccountCredential>(this.googleDriveServiceAccountKeyPath)
                .ToGoogleCredential()
                .CreateScoped(DriveService.Scope.DriveFile);

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "PayTrack",
            });
        }

        private async Task<string> GetOrCreateFolderAsync(DriveService driveService, string folderName, string parentFolderId)
        {
            var listRequest = driveService.Files.List();
            listRequest.Q = $"name = '{EscapeDriveQueryValue(folderName)}' and mimeType = 'application/vnd.google-apps.folder' and '{EscapeDriveQueryValue(parentFolderId)}' in parents and trashed = false";
            listRequest.Fields = "files(id, name)";
            listRequest.PageSize = 1;

            var folders = await listRequest.ExecuteAsync();
            var existingFolder = folders.Files.FirstOrDefault();

            if (existingFolder?.Id != null)
            {
                return existingFolder.Id;
            }

            var folderMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = [parentFolderId],
            };

            var createRequest = driveService.Files.Create(folderMetadata);
            createRequest.Fields = "id";

            var createdFolder = await createRequest.ExecuteAsync();
            return createdFolder.Id ?? throw new InternalErrorException("Creating Google Drive folder failed.");
        }
    }
}
