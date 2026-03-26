// <copyright file="Role.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Data.Entities
{
    /// <summary>
    /// Role of User in the System.
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// Regular System User. No particular rights
        /// </summary>
        RegularUser,

        /// <summary>
        /// Similar to the RegularUser but also has access to his Teams Budgets
        /// </summary>
        TeamLead,

        /// <summary>
        /// Admin Role. Has the most rights
        /// </summary>
        Admin,
    }
}
