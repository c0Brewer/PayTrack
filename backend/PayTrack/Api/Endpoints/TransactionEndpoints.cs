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
                .RequireAuthorization();

            group.MapGet("/user", PaymentRequestByUserHandler.GetPaymentRequestByUsersAsync);

            group.MapGet("/user/{id:int}", PaymentRequestByUserHandler.GetPaymentRequestByUserByIdAsync);

            group.MapPost("/user", PaymentRequestByUserHandler.CreatePaymentRequestByUserAsync).DisableAntiforgery(); // Needed because of the way the file upload works. This is intentional

            group.MapPut("/user/{id:int}", PaymentRequestByUserHandler.UpdatePaymentRequestByUserAsync)
                .RequireRole(Role.Admin);

            group.MapPost("/user/{id:int}/mark-paid", PaymentRequestByUserHandler.MarkPaymentRequestByUserAsPaidAsync)
                .RequireRole(Role.Admin);

            group.MapPost("/user/{id:int}/approve", PaymentRequestByUserHandler.ApprovePaymentRequestByUserAsync)
                .RequireRole(Role.Admin);

            group.MapPost("/user/{id:int}/decline", PaymentRequestByUserHandler.DeclinePaymentRequestByUserAsync)
                .RequireRole(Role.Admin);

            group.MapPost("/user/{id:int}/request-changes", PaymentRequestByUserHandler.RequestChangesPaymentRequestByUserAsync)
                .RequireRole(Role.Admin);

            group.MapPost("/user/{id:int}/undo-status-change", PaymentRequestByUserHandler.UndoLastPaymentRequestByUserStatusChangeAsync)
                .RequireRole(Role.Admin);

            group.MapGet("/user/{id:int}/receipt", PaymentRequestByUserHandler.GetPaymentRequestByUserByIdReceiptAsync);

            group.MapGet("/team", PaymentRequestByTeamHandler.GetPaymentRequestByTeamsAsync);
            group.MapGet("/team/{id:int}", PaymentRequestByTeamHandler.GetPaymentRequestByTeamByIdAsync);
            group.MapPost("/team", PaymentRequestByTeamHandler.CreatePaymentRequestByTeamAsync);
            group.MapPut("/team/{id:int}", PaymentRequestByTeamHandler.UpdatePaymentRequestByTeamAsync)
                .RequireRole(Role.Admin);

            group.MapGet("/user/duplicate", PaymentRequestByUserHandler.GetDuplicatePaymentRequestsByUserAsync);

            //Send the same information as in `create`, but without the file
            //Return if it matches -> empty list
            //If there are duplicates -> list of duplicates (maximum 10)
            //Custom DTO for duplicates in `PaymentRequestByUser GetDuplicatePaymentRequestsByUserDto`
            //Invoice number, Amount (perhaps a fuzzy matcher)
        }
    }
}
