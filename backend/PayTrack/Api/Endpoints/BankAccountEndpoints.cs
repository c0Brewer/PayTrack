// <copyright file="BankAccountEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Handler;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for Bank Accounts.
    /// </summary>
    public static class BankAccountEndpoints
    {
        private const string GroupName = "BankAccount";
        private const string GroupRoute = "bankaccount";

        /// <summary>
        /// Maps the Endpoints necessary for Bank Accounts.
        /// </summary>
        /// <param name="app">Web application.</param>
        public static void MapBankAccountEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization();

            group.MapGet("/", BankAccountHandler.GetBankAccountsAsync);
            group.MapPost("/", BankAccountHandler.CreateBankAccountAsync);
            group.MapPut("/{id:int}", BankAccountHandler.UpdateBankAccountAsync);
            group.MapDelete("/{id:int}", BankAccountHandler.DeleteBankAccountAsync);
        }
    }
}
