// <copyright file="EndpointAuthorizationExtensions.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;
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
        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
        public static RouteGroupBuilder RequireRole(
            this RouteGroupBuilder builder,
            Role role)
        {
            builder.RequireAuthorization(role.ToString());
            return builder;
        }

        /// <summary>
        /// Blocks deactivated users from accessing an Endpoint.
        /// </summary>
        /// <param name="builder">builder auto-injected.</param>
        /// <returns>Route Handler Builder.</returns>
        [ExcludeFromCodeCoverage]
        public static RouteHandlerBuilder RequireActiveUser(this RouteHandlerBuilder builder)
        {
            return builder.AddEndpointFilter(async (context, next) =>
            {
                var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                var user = await authService.GetCurrentUser();
                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }

                if (!user.IsActive)
                {
                    throw new ForbiddenException("Your account is deactivated.");
                }

                return await next(context);
            });
        }

        /// <summary>
        /// Blocks deactivated users from accessing a Route Group.
        /// </summary>
        /// <param name="builder">builder auto-injected.</param>
        /// <returns>Route Group Builder.</returns>
        [ExcludeFromCodeCoverage]
        public static RouteGroupBuilder RequireActiveUser(this RouteGroupBuilder builder)
        {
            builder.AddEndpointFilter(async (context, next) =>
            {
                var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
                var user = await authService.GetCurrentUser();
                if (user == null)
                {
                    throw new NotFoundException("User not found.");
                }

                if (!user.IsActive)
                {
                    throw new ForbiddenException("Your account is deactivated.");
                }

                return await next(context);
            });
            return builder;
        }
    }
}
