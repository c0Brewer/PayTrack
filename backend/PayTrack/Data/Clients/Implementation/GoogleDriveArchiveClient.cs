// <copyright file="GoogleDriveArchiveClient.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Clients.Model;

namespace PayTrack.Data.Clients.Implementation
{
    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public class GoogleDriveArchiveClient(IConfiguration _config) : IGoogleDriveArchiveClient
    {
        private readonly string? googleDriveRootFolderId = _config["GoogleDrive:RootFolderId"];
        private readonly string? googleDriveServiceAccountKeyPath = _config["GoogleDrive:ServiceAccountKeyPath"];
        private readonly string? googleDriveServiceAccountKeyBase64 = _config["GoogleDrive:ServiceAccountKeyBase64"];

        /// <inheritdoc/>
        public async Task ArchiveAsync(string localFilePath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(this.googleDriveRootFolderId))
            {
                throw new InternalErrorException("Google Drive root folder id is not configured.");
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
            request.SupportsAllDrives = true;

            var result = await request.UploadAsync();

            if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                var errorMessage = result.Exception?.Message ?? result.Status.ToString();
                throw new InternalErrorException($"Uploading invoice to Google Drive failed: {errorMessage}");
            }
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
        /// Decodes a base64-encoded JSON credential value into a readable stream.
        /// </summary>
        /// <param name="value">The base64-encoded credential JSON.</param>
        /// <param name="errorMessage">The error message used when the value cannot be decoded.</param>
        /// <returns>A stream containing the decoded JSON credential.</returns>
        private static MemoryStream CreateStreamFromBase64(string value, string errorMessage)
        {
            try
            {
                return new MemoryStream(Convert.FromBase64String(value));
            }
            catch (FormatException exception)
            {
                throw new InternalErrorException($"{errorMessage} {exception.Message}");
            }
        }

        /// <summary>
        /// Creates an authenticated Google Drive service using the configured service account key.
        /// </summary>
        /// <returns>An authenticated <see cref="DriveService"/> instance.</returns>
        private DriveService CreateDriveService()
        {
            GoogleCredential credential;
            var serviceAccountKeyBase64 = this.googleDriveServiceAccountKeyBase64;
            var serviceAccountKeyPath = this.googleDriveServiceAccountKeyPath;
            var hasServiceAccountKeyBase64 = !string.IsNullOrWhiteSpace(serviceAccountKeyBase64);
            var hasServiceAccountKeyPath = !string.IsNullOrWhiteSpace(serviceAccountKeyPath);

            if (hasServiceAccountKeyBase64 && hasServiceAccountKeyPath)
            {
                throw new InternalErrorException("Google Drive service account key path and base64 value cannot both be configured.");
            }

            if (hasServiceAccountKeyBase64)
            {
                using var stream = CreateStreamFromBase64(
                    serviceAccountKeyBase64!,
                    "Google Drive service account key base64 value is invalid.");

                credential = CredentialFactory
                    .FromStream<ServiceAccountCredential>(stream)
                    .ToGoogleCredential()
                    .CreateScoped(DriveService.Scope.Drive);
            }
            else
            {
                if (!hasServiceAccountKeyPath)
                {
                    throw new InternalErrorException("Google Drive service account key is not configured.");
                }

                if (!File.Exists(serviceAccountKeyPath))
                {
                    throw new InternalErrorException("Google Drive service account key file could not be found.");
                }

                credential = CredentialFactory
                    .FromFile<ServiceAccountCredential>(serviceAccountKeyPath)
                    .ToGoogleCredential()
                    .CreateScoped(DriveService.Scope.Drive);
            }

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
        private async Task<string> GetOrCreateFolderAsync(DriveService driveService, string folderName, string parentFolderId)
        {
            var listRequest = driveService.Files.List();
            listRequest.Q = $"name = '{EscapeDriveQueryValue(folderName)}' and mimeType = 'application/vnd.google-apps.folder' and '{EscapeDriveQueryValue(parentFolderId)}' in parents and trashed = false";
            listRequest.Fields = "files(id, name)";
            listRequest.PageSize = 1;
            listRequest.IncludeItemsFromAllDrives = true;
            listRequest.SupportsAllDrives = true;

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
            createRequest.SupportsAllDrives = true;

            var createdFolder = await createRequest.ExecuteAsync();
            return createdFolder.Id ?? throw new InternalErrorException("Creating Google Drive folder failed.");
        }
    }
}
