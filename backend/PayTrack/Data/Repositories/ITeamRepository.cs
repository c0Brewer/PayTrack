// <copyright file="ITeamRepository.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Data.Repositories;

/// <summary>
/// Repository for all Team-related operations.
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Returns all Teams from DB.
    /// </summary>
    /// <returns>List of Team objects.</returns>
    Task<List<Team>> GetAllAsync();

    /// <summary>
    /// Gets a specific Team by their ID.
    /// </summary>
    /// <param name="id">id of Team to find.</param>
    /// <returns>Team with given ID.</returns>
    Task<Team?> GetByIdAsync(int id);

    /// <summary>
    /// Stores a Team to the Database.
    /// </summary>
    /// <param name="team">Team object to store.</param>
    /// <returns>Instance of created Team object.</returns>
    Task<Team> AddAsync(Team team);
}
