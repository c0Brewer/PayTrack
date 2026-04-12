// <copyright file="TransactionEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Extensions;
using PayTrack.Api.Handler;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for Transaction.
    /// </summary>
    public static class TransactionEndpoints
    {
        private const string GroupName = "Transactions";
        private const string GroupRoute = "transaction";

        /// <summary>
        /// Maps the Endpoints necessary for Transactions.
        /// </summary>
        /// <param name="app">Webapplication.</param>
        public static void MapTransactionEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization()
                .RequireRole(Role.Admin);

            group.MapGet("/user/", PaymentRequestByUserHandler.GetPaymentRequestByUsersAsync);
            group.MapGet("/user/{id:int}", PaymentRequestByUserHandler.GetPaymentRequestByUserByIdAsync);
            group.MapPut("/user/{id:int}", PaymentRequestByUserHandler.UpdatePaymentRequestByUserAsync);
            group.MapPost("/user/{id:int}", PaymentRequestByUserHandler.CreatePaymentRequestByUserAsync);

            // TODO: Implement this:

            // group.MapGet("/team/", PaymentRequestByTeamHandler.GetPaymentRequestByTeamsAsync);
            // group.MapGet("/team/{id:int}", PaymentRequestByTeamHandler.GetPaymentRequestByTeamByIdAsync);
            // group.MapPut("/team/{id:int}", PaymentRequestByTeamHandler.UpdatePaymentRequestByTeamAsync);
        }
    }
}
