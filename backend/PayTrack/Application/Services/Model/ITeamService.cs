// <copyright file="ITeamService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Team;
using PayTrack.Data.Entities;

namespace PayTrack.Application.Services.Model
{
    /// <summary>
    /// Service which handles Team-related requests.
    /// </summary>
    public interface ITeamService
    {
        /// <summary>
        /// Returns all Teams from DB.
        /// </summary>
        /// <param name="query">Query information for search.</param>
        /// <returns>List of Team objects.</returns>
        Task<(List<Team> team, int totalCount)> GetTeamsAsync(GetTeamQuery? query = null);

        /// <summary>
        /// Gets a specific Team by their ID.
        /// </summary>
        /// <param name="id">id of Team to find.</param>
        /// <param name="query">Query information for search.</param>
        /// <returns>Team with given ID.</returns>
        Task<Team?> GetTeamByIdAsync(int id, GetTeamQueryById? query = null);

        /// <summary>
        /// Creates a Team using the given input.
        /// </summary>
        /// <param name="name">name of team.</param>
        /// <param name="description">description of team.</param>
        /// <param name="displayColor">displayColor of team.</param>
        /// <returns>Instance of created Team object.</returns>
        Task<Team> CreateTeamAsync(string name, string? description, string? displayColor);
    }
}
