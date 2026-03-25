// <copyright file="Team.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Data.Entities;

/// <summary>
/// Entity representing a Team.
/// </summary>
public class Team
{
    /// <summary>
    /// Id of the Team.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the Team.
    /// </summary>
    public string Name { get; set; } = default!;
}
