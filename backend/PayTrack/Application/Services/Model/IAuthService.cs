// <copyright file="IAuthService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Auth;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles Auth-related requests.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Returns all Auths from DB.
        /// </summary>
        /// <param name="googleCallback">Callback Response from Google.</param>
        /// <returns>List of Auth objects.</returns>
        Task<string> GoogleValidateCallback(GoogleAuthCallbackDto googleCallback);
    }
}
