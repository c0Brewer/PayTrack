// <copyright file="AdminSettingsEndpoints.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Api.Extensions;
using PayTrack.Api.Handler;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Endpoints
{
    /// <summary>
    /// Contains admin-only endpoints for managing runtime-configurable system settings.
    /// </summary>
    public static class AdminSettingsEndpoints
    {
        private const string GroupName = "AdminSettings";
        private const string GroupRoute = "admin/settings";

        /// <summary>
        /// Maps the admin settings endpoints.
        /// </summary>
        /// <param name="app">WebApplication.</param>
        public static void MapAdminSettingsEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app
                .MapGroup($"/{GroupRoute}")
                .WithTags(GroupName)
                .RequireAuthorization()
                .RequireRole(Role.Admin)
                .RequireActiveUser();

            group.MapGet("/csv-columns", AdminSettingsHandler.GetCsvColumnSettingsAsync);
            group.MapPut("/csv-columns", AdminSettingsHandler.UpdateCsvColumnSettingsAsync);

            group.MapGet("/notification-channels", AdminSettingsHandler.GetNotificationChannelGroupsAsync);
            group.MapPut("/notification-channels", AdminSettingsHandler.UpdateNotificationChannelGroupsAsync);

            group.MapGet("/reminder-schedule", AdminSettingsHandler.GetReminderScheduleAsync);
            group.MapPut("/reminder-schedule", AdminSettingsHandler.UpdateReminderScheduleAsync);

            group.MapGet("/invoice-submission", AdminSettingsHandler.GetInvoiceSubmissionSettingsAsync);
            group.MapPut("/invoice-submission", AdminSettingsHandler.UpdateInvoiceSubmissionSettingsAsync);

            app.MapGet($"/{GroupRoute}/invoice-submission/public", AdminSettingsHandler.GetInvoiceSubmissionSettingsAsync)
                .WithTags(GroupName)
                .RequireAuthorization()
                .RequireActiveUser();
        }
    }
}
