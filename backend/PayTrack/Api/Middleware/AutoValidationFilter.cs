// <copyright file="AutoValidationFilter.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace PayTrack.Api.Middleware
{
    /// <summary>
    /// Middleware which automatically triggers validation of dto.
    /// </summary>
    public class AutoValidationFilter : IEndpointFilter
    {
        /// <summary>
        /// 123.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="next">Next.</param>
        /// <returns>Returns endpoint filter.</returns>
        public async ValueTask<object?> InvokeAsync(
            EndpointFilterInvocationContext context,
            EndpointFilterDelegate next)
        {
            foreach (var arg in context.Arguments)
            {
                if (arg == null)
                {
                    continue;
                }

                var validationContext = new ValidationContext(arg);
                var results = new List<ValidationResult>();

                // validate object using DataAnnotations
                if (!Validator.TryValidateObject(arg, validationContext, results, true))
                {
                    return Results.BadRequest(results);
                }
            }

            return await next(context);
        }
    }
}
