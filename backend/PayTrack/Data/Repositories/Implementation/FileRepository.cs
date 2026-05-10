// <copyright file="FileRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class FileRepository(IConfiguration _config) : IFileRepository
    {
        private readonly string fileUploadPath = _config["Data:FileUploadPath"] ?? throw new InternalErrorException("Could not load file upload path.");
        private readonly bool googleDriveEnabled = _config.GetValue("GoogleDrive:Enabled", false);
        private readonly string googleDriveAuthenticationMode = _config["GoogleDrive:AuthenticationMode"] ?? "ServiceAccount";
        private readonly string? googleDriveRootFolderId = _config["GoogleDrive:RootFolderId"];
        private readonly string? googleDriveServiceAccountKeyPath = _config["GoogleDrive:ServiceAccountKeyPath"];
        private readonly string? googleDriveOAuthClientSecretsPath = _config["GoogleDrive:OAuthClientSecretsPath"];
        private readonly string googleDriveOAuthTokenStorePath = _config["GoogleDrive:OAuthTokenStorePath"] ?? "google-drive-token";

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

        /// <summary>
        /// Escapes special characters in a value used inside a Google Drive API query string.
        /// </summary>
        /// <param name="value">The raw query value.</param>
        /// <returns>The escaped query value.</returns>
        private static string EscapeDriveQueryValue(string value)
        {
            return value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal);
        }

        /// <summary>
        /// Resolves the MIME content type for a file name based on its extension.
        /// </summary>
        /// <param name="fileName">The file name whose extension should be evaluated.</param>
        /// <returns>The matching MIME type, or <c>application/octet-stream</c> when the extension is unknown.</returns>
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

        /// <summary>
        /// Uploads a locally stored invoice file to the configured Google Drive archive folder.
        /// </summary>
        /// <param name="localFilePath">The path of the locally stored file.</param>
        /// <param name="fileName">The file name to use in Google Drive.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous upload operation.</returns>
        private async Task UploadToGoogleDriveAsync(string localFilePath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(this.googleDriveRootFolderId))
            {
                throw new InternalErrorException("Google Drive root folder id is not configured.");
            }

            using var driveService = await this.CreateDriveServiceAsync();
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
                var errorMessage = result.Exception?.Message ?? result.Status.ToString();
                throw new InternalErrorException($"Uploading invoice to Google Drive failed: {errorMessage}");
            }
        }

        /// <summary>
        /// Creates an authenticated Google Drive service using the configured authentication mode.
        /// </summary>
        /// <returns>An authenticated <see cref="DriveService"/> instance.</returns>
        [ExcludeFromCodeCoverage]
        private async Task<DriveService> CreateDriveServiceAsync()
        {
            return this.googleDriveAuthenticationMode.ToLowerInvariant() switch
            {
                "oauth" => await this.CreateOAuthDriveServiceAsync(),
                "serviceaccount" => this.CreateServiceAccountDriveService(),
                _ => throw new InternalErrorException("Google Drive authentication mode is not supported."),
            };
        }

        /// <summary>
        /// Creates an authenticated Google Drive service using the configured service account key file.
        /// </summary>
        /// <returns>An authenticated <see cref="DriveService"/> instance.</returns>
        [ExcludeFromCodeCoverage]
        private DriveService CreateServiceAccountDriveService()
        {
            if (string.IsNullOrWhiteSpace(this.googleDriveServiceAccountKeyPath))
            {
                throw new InternalErrorException("Google Drive service account key path is not configured.");
            }

            if (!File.Exists(this.googleDriveServiceAccountKeyPath))
            {
                throw new InternalErrorException("Google Drive service account key file could not be found.");
            }

            var credential = CredentialFactory
                .FromFile<ServiceAccountCredential>(this.googleDriveServiceAccountKeyPath)
                .ToGoogleCredential()
                .CreateScoped(DriveService.Scope.Drive);

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "PayTrack",
            });
        }

        /// <summary>
        /// Creates an authenticated Google Drive service using an OAuth user consent flow.
        /// </summary>
        /// <returns>An authenticated <see cref="DriveService"/> instance.</returns>
        [ExcludeFromCodeCoverage]
        private async Task<DriveService> CreateOAuthDriveServiceAsync()
        {
            if (string.IsNullOrWhiteSpace(this.googleDriveOAuthClientSecretsPath))
            {
                throw new InternalErrorException("Google Drive OAuth client secrets path is not configured.");
            }

            if (!File.Exists(this.googleDriveOAuthClientSecretsPath))
            {
                throw new InternalErrorException("Google Drive OAuth client secrets file could not be found.");
            }

            await using var stream = File.OpenRead(this.googleDriveOAuthClientSecretsPath);
            var clientSecrets = GoogleClientSecrets.FromStream(stream).Secrets;
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                [DriveService.Scope.Drive],
                "paytrack-drive-archive",
                CancellationToken.None,
                new FileDataStore(this.googleDriveOAuthTokenStorePath, true));

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "PayTrack",
            });
        }

        /// <summary>
        /// Finds an existing child folder in Google Drive or creates it when it does not exist.
        /// </summary>
        /// <param name="driveService">The authenticated Google Drive service.</param>
        /// <param name="folderName">The child folder name.</param>
        /// <param name="parentFolderId">The Google Drive ID of the parent folder.</param>
        /// <returns>The Google Drive ID of the existing or newly created folder.</returns>
        [ExcludeFromCodeCoverage]
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
