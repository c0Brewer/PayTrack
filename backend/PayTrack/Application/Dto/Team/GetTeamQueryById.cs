// <copyright file="GetTeamQueryById.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Application.Dto.Team;

/// <summary>
/// Data Transfer Object (DTO) representing all information a team can query on GET /team/{id}.
/// </summary>
public class GetTeamQueryById
{
    /// <summary>
    /// Whether to include team members.
    /// </summary>
    public bool? IncludeMembers { get; init; }

    /// <summary>
    /// Whether to include budgets.
    /// </summary>
    public bool? IncludeBudgets { get; init; }
}