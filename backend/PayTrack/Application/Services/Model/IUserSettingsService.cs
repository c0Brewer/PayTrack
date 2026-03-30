// <copyright file="IUserSettingsService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.UserSettings;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles User Settings-related requests.
    /// </summary>
    public interface IUserSettingsService
    {
        /// <summary>
        /// Gets the user settings for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>User settings DTO.</returns>
        Task<UserSettingsDto> GetUserSettingsAsync(int userId);

        /// <summary>
        /// Updates the user settings for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="settingsDto">The new user settings.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateUserSettingsAsync(int userId, UserSettingsDto settingsDto);
    }
}