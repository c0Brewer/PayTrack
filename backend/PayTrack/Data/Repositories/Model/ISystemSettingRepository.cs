// <copyright file="ISystemSettingRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for reading and writing admin-configurable system settings.
    /// </summary>
    public interface ISystemSettingRepository
    {
        /// <summary>
        /// Returns the setting row for the given key, or null if no row exists.
        /// </summary>
        /// <param name="key">Setting key.</param>
        /// <returns>The <see cref="SystemSetting"/> or null.</returns>
        Task<SystemSetting?> GetByKeyAsync(string key);

        /// <summary>
        /// Inserts or updates the setting row for the given key.
        /// </summary>
        /// <param name="key">Setting key.</param>
        /// <param name="value">Serialized setting value.</param>
        /// <param name="lastModifiedByUserId">ID of the admin performing the update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpsertAsync(string key, string value, int lastModifiedByUserId);

        /// <summary>
        /// Inserts or updates all settings in the dictionary in a single atomic operation.
        /// </summary>
        /// <param name="settings">Key-value pairs to upsert.</param>
        /// <param name="lastModifiedByUserId">ID of the admin performing the update.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task UpsertManyAsync(IReadOnlyDictionary<string, string> settings, int lastModifiedByUserId);
    }
}
