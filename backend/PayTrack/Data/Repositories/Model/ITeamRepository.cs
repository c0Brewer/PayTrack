// <copyright file="ITeamRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Team;
using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories.Model
{
    /// <summary>
    /// Repository for all Team-related operations.
    /// </summary>
    public interface ITeamRepository
    {
        /// <summary>
        /// Returns all Teams from DB.
        /// </summary>
        /// <param name="query">Query information to include in search.</param>
        /// <returns>List of Team objects.</returns>
        Task<(List<Team> team, int totalCount)> GetAllAsync(GetTeamQuery? query = null);

        /// <summary>
        /// Gets a specific Team by their ID.
        /// </summary>
        /// <param name="id">id of Team to find.</param>
        /// <param name="query">Query information to include in search.</param>
        /// <returns>Team with given ID.</returns>
        Task<Team?> GetByIdAsync(int id, GetTeamQueryById? query = null);

        /// <summary>
        /// Stores a Team to the Database.
        /// </summary>
        /// <param name="team">Team object to store.</param>
        /// <returns>Instance of created Team object.</returns>
        Task<Team> AddAsync(Team team);
    }
}
