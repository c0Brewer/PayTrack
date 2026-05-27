// <copyright file="FileRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Clients.Model;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class FileRepository(
        IConfiguration _config,
        IGoogleDriveArchiveClient _googleDriveArchiveClient,
        INotificationDispatchService _notificationDispatchService,
        ILogger<FileRepository> _logger) : IFileRepository
    {
        private const string GoogleDriveArchiveFailureRecipient = "gitlab@racing.tuwien.ac.at";

        private readonly string fileUploadPath = _config["Data:FileUploadPath"] ?? throw new InternalErrorException("Could not load file upload path.");
        private readonly bool googleDriveEnabled = _config.GetValue("GoogleDrive:Enabled", false);

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
                try
                {
                    await _googleDriveArchiveClient.ArchiveAsync(filePath, safeFileName);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Archiving invoice {FileName} to Google Drive failed after local save.",
                        safeFileName);

                    await this.NotifyAboutArchiveFailureAsync(filePath, safeFileName, file.ContentType, exception);
                }
            }

            return filePath;
        }

        private async Task NotifyAboutArchiveFailureAsync(
            string localFilePath,
            string fileName,
            string contentType,
            Exception archiveException)
        {
            try
            {
                var attachment = new EmailAttachment(
                    fileName,
                    await File.ReadAllBytesAsync(localFilePath),
                    string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
                var attachments = new[] { attachment };
                var subject = $"Google Drive invoice archive failed: {fileName}";
                var body = $"""
                    PayTrack stored the invoice locally, but uploading it to Google Drive failed.

                    Local file path: {localFilePath}
                    File name: {fileName}
                    Error: {archiveException.Message}

                    The invoice is attached so it can be uploaded manually.
                    """;

                await _notificationDispatchService.SendEmailAsync(
                    GoogleDriveArchiveFailureRecipient,
                    subject,
                    body,
                    attachments);
            }
            catch (Exception notificationException)
            {
                _logger.LogError(
                    notificationException,
                    "Sending Google Drive archive failure notification for invoice {FileName} failed.",
                    fileName);
            }
        }
    }
}
