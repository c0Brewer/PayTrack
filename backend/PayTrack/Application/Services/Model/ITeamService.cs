// <copyright file="ITeamService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Budget;
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
        /// <param name="budgetEntries">Optional budgets to create together with the team.</param>
        /// <returns>Instance of created Team object.</returns>
        Task<Team> CreateTeamAsync(
            string name,
            string? description,
            string? displayColor,
            IList<CreateTeamBudgetEntryDto>? budgetEntries);

        /// <summary>
        /// Update a Team using the given input.
        /// </summary>
        /// <param name="id">The id of the team to update.</param>
        /// <param name="name">The new name that should be set for the team.</param>
        /// <param name="description">The new description that should be set for the team.</param>
        /// <param name="displayColor">The new display color that should be set for the team.</param>
        /// <param name="budgetsToUpsert">Optional budgets to create or update for the team.</param>
        /// <param name="budgetIdsToDelete">Optional budget ids to remove from the team.</param>
        /// <returns>Instance of created Team object.</returns>
        Task<Team> UpdateTeamAsync(
            int id,
            string? name,
            string? description,
            string? displayColor,
            IList<UpsertTeamBudgetEntryDto>? budgetsToUpsert,
            IList<int>? budgetIdsToDelete);

        /// <summary>
        /// Deletes a Team by id.
        /// </summary>
        /// <param name="id">The id of the team to delete.</param>
        /// <returns>Deleted team instance.</returns>
        Task<Team> DeleteTeamAsync(int id);

        /// <summary>
        /// Gets a preview of the impact of deleting a Team.
        /// </summary>
        /// <param name="id">The id of the team to inspect.</param>
        /// <returns>Delete impact information for the requested team.</returns>
        Task<DeleteTeamImpactDto?> GetDeleteTeamImpactAsync(int id);
    }
}
