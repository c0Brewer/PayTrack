// <copyright file="IFileRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for all File-related operations.
    /// </summary>
    public interface IFileRepository
    {
        /// <summary>
        /// Gets a locally stored file by its path.
        /// </summary>
        /// <param name="filePath">Path of the file to read.</param>
        /// <returns>The file content as bytes.</returns>
        Task<byte[]> GetByPath(string filePath);

        /// <summary>
        /// Stores a file locally and archives it to Google Drive when Drive archiving is enabled.
        /// </summary>
        /// <param name="file">File to store.</param>
        /// <param name="name">Base name to store the file under, without the original extension.</param>
        /// <returns>Local path of where the file was stored.</returns>
        Task<string> SaveFile(IFormFile file, string name);
    }
}
