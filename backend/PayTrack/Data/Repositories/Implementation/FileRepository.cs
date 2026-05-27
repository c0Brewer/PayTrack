// <copyright file="FileRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Exceptions;
using PayTrack.Data.Clients.Model;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Data.Repositories.Implementation
{
    /// <inheritdoc/>
    public class FileRepository(IConfiguration _config, IGoogleDriveArchiveClient _googleDriveArchiveClient) : IFileRepository
    {
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
                await _googleDriveArchiveClient.ArchiveAsync(filePath, safeFileName);
            }

            return filePath;
        }
    }
}
