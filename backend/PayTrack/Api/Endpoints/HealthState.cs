// <copyright file="HealthState.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Shared state for readiness and graceful shutdown endpoints.
    /// </summary>
    public sealed class HealthState
    {
        /// <summary>
        /// Gets or sets a value indicating whether shutdown has been requested.
        /// </summary>
        public bool ShutdownRequested { get; set; }
    }
}
