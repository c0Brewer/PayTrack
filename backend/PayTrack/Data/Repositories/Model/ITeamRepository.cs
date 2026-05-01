// <copyright file="ITeamRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
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
        /// <param name="budgetEntries">Optional budgets to create together with the team.</param>
        /// <returns>Instance of created Team object.</returns>
        Task<Team> AddAsync(Team team, IList<CreateTeamBudgetEntryDto>? budgetEntries = null);

        /// <summary>
        /// Updates a Team with optional values.
        /// </summary>
        /// <param name="id">Id of Team to update.</param>
        /// <param name="name">Name to optionally set.</param>
        /// <param name="description">Description to optionally set.</param>
        /// <param name="displayColor">Display color to optionally set.</param>
        /// <param name="budgetsToUpsert">Optional budgets to create or update for the team.</param>
        /// <param name="budgetIdsToDelete">Optional budget ids to remove from the team.</param>
        /// <returns>Updated Team instance.</returns>
        Task<Team> UpdateAsync(
            int id,
            string? name,
            string? description,
            string? displayColor,
            IList<UpsertTeamBudgetEntryDto>? budgetsToUpsert,
            IList<int>? budgetIdsToDelete);

        /// <summary>
        /// Deletes a Team by id.
        /// </summary>
        /// <param name="id">Id of Team to delete.</param>
        /// <returns>Deleted Team instance.</returns>
        Task<Team> DeleteAsync(int id);

        /// <summary>
        /// Gets the impact of deleting a Team.
        /// </summary>
        /// <param name="id">Id of Team to inspect.</param>
        /// <returns>Delete impact information for the requested team.</returns>
        Task<DeleteTeamImpactDto?> GetDeleteTeamImpactAsync(int id);
    }
}
