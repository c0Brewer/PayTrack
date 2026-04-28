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
        /// Returns all Teams from DB.
        /// </summary>
        /// <param name="filePath">Name of file to find.</param>
        /// <returns>File.</returns>
        Task<byte[]> GetByPath(string filePath);

        /// <summary>
        /// Gets a specific Team by their ID.
        /// </summary>
        /// <param name="file">File to store.</param>
        /// <param name="name">Name to store file under.</param>
        /// <returns>Path of where the file was stored.</returns>
        Task<string> SaveFile(IFormFile file, string name);
    }
}
