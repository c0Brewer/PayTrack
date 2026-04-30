// <copyright file="MyInvoicesEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Handler;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains Endpoints for a user's own invoices.
    /// </summary>
    public static class MyInvoicesEndpoints
    {
        private const string GroupName = "MyInvoices";
        private const string GroupRoute = "my-invoices";

        /// <summary>
        /// Maps the Endpoints necessary for a user's own invoices.
        /// </summary>
        /// <param name="app">Webapplication.</param>
        public static void MapMyInvoicesEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization();

            group.MapGet("/", MyInvoicesHandler.GetMyInvoicesAsync);

            group.MapGet("/{id:int}", MyInvoicesHandler.GetMyInvoiceByIdAsync);

            group.MapGet("/{id:int}/receipt", MyInvoicesHandler.GetMyInvoiceReceiptAsync);
        }
    }
}
