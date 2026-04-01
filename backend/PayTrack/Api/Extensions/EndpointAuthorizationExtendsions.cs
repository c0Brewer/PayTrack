// <copyright file="EndpointAuthorizationExtendsions.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Data.Entities;

namespace PayTrack.Api.Extensions
{
    /// <summary>
    /// Class containing extensions for the endpoints.
    /// </summary>
    public static class EndpointAuthorizationExtensions
    {
        /// <summary>
        /// Adds a minimum required Role to an Endpoint.
        /// </summary>
        /// <param name="builder">builder auto-injected.</param>
        /// <param name="role">Minimum Role.</param>
        /// <returns>Route Handler Builder.</returns>
        public static RouteHandlerBuilder RequireRole(
            this RouteHandlerBuilder builder,
            Role role)
        {
            return builder.RequireAuthorization(role.ToString());
        }

        /// <summary>
        /// Adds a minimum required Role to an Endpoint.
        /// </summary>
        /// <param name="builder">builder auto-injected.</param>
        /// <param name="role">Minimum Role.</param>
        /// <returns>Route Handler Builder.</returns>
        public static RouteGroupBuilder RequireRole(
            this RouteGroupBuilder builder,
            Role role)
        {
            builder.RequireAuthorization(role.ToString());
            return builder;
        }
    }
}
