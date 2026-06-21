// <copyright file="AdminSettingsHandler.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PayTrack.Application.Dto.AdminSettings;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Model;

namespace PayTrack.Api.Handler
{
    /// <summary>
    /// Handler for admin-configurable system setting endpoints.
    /// </summary>
    public static class AdminSettingsHandler
    {
        /// <summary>
        /// Returns the current CSV column name settings.
        /// </summary>
        /// <param name="service">Dependency-injected system setting service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<CsvColumnSettingsDto>, ProblemHttpResult>> GetCsvColumnSettingsAsync(
            ISystemSettingService service)
        {
            var dto = await service.GetCsvColumnSettingsAsync();
            return TypedResults.Ok(dto);
        }

        /// <summary>
        /// Updates the CSV column name settings.
        /// </summary>
        /// <param name="request">New column names.</param>
        /// <param name="service">Dependency-injected system setting service.</param>
        /// <param name="authService">Dependency-injected auth service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, NotFound<ProblemDetails>, ProblemHttpResult>> UpdateCsvColumnSettingsAsync(
            [Microsoft.AspNetCore.Mvc.FromBody] UpdateCsvColumnSettingsRequestDto request,
            ISystemSettingService service,
            IAuthService authService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("User not found.");
            await service.UpdateCsvColumnSettingsAsync(request, user.Id);
            return TypedResults.NoContent();
        }

        /// <summary>
        /// Returns the current notification channel group settings.
        /// </summary>
        /// <param name="service">Dependency-injected system setting service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<NotificationChannelGroupsDto>, ProblemHttpResult>> GetNotificationChannelGroupsAsync(
            ISystemSettingService service)
        {
            var dto = await service.GetNotificationChannelGroupsAsync();
            return TypedResults.Ok(dto);
        }

        /// <summary>
        /// Updates the notification channel group settings.
        /// </summary>
        /// <param name="request">New channel settings.</param>
        /// <param name="service">Dependency-injected system setting service.</param>
        /// <param name="authService">Dependency-injected auth service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, NotFound<ProblemDetails>, ProblemHttpResult>> UpdateNotificationChannelGroupsAsync(
            [Microsoft.AspNetCore.Mvc.FromBody] UpdateNotificationChannelGroupsRequestDto request,
            ISystemSettingService service,
            IAuthService authService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("User not found.");
            await service.UpdateNotificationChannelGroupsAsync(request, user.Id);
            return TypedResults.NoContent();
        }

        /// <summary>
        /// Returns the current reminder schedule settings.
        /// </summary>
        /// <param name="service">Dependency-injected system setting service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<Ok<ReminderScheduleDto>, ProblemHttpResult>> GetReminderScheduleAsync(
            ISystemSettingService service)
        {
            var dto = await service.GetReminderScheduleAsync();
            return TypedResults.Ok(dto);
        }

        /// <summary>
        /// Updates the reminder schedule settings.
        /// </summary>
        /// <param name="request">New schedule settings.</param>
        /// <param name="service">Dependency-injected system setting service.</param>
        /// <param name="authService">Dependency-injected auth service.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<Results<NoContent, NotFound<ProblemDetails>, ProblemHttpResult>> UpdateReminderScheduleAsync(
            [Microsoft.AspNetCore.Mvc.FromBody] UpdateReminderScheduleRequestDto request,
            ISystemSettingService service,
            IAuthService authService)
        {
            var user = await authService.GetCurrentUser() ?? throw new NotFoundException("User not found.");
            await service.UpdateReminderScheduleAsync(request, user.Id);
            return TypedResults.NoContent();
        }
    }
}
