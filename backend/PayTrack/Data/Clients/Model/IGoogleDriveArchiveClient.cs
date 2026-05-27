// <copyright file="IGoogleDriveArchiveClient.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Clients.Model
{
    /// <summary>
    /// Archives locally stored files to Google Drive.
    /// </summary>
    public interface IGoogleDriveArchiveClient
    {
        /// <summary>
        /// Uploads a locally stored file to the configured Google Drive archive folder.
        /// </summary>
        /// <param name="localFilePath">The path of the locally stored file.</param>
        /// <param name="fileName">The file name to use in Google Drive.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous upload operation.</returns>
        Task ArchiveAsync(string localFilePath, string fileName);
    }
}
