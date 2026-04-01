// <copyright file="PaginatedResponse.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace PayTrack.Application.Dto.Pagination
{
    /// <summary>
    /// Wrapper for paginated responses.
    /// </summary>
    /// <typeparam name="T">Type of Response to paginate.</typeparam>
    public class PaginatedResponse<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedResponse{T}"/> class.
        /// </summary>
        /// <param name="items">Items retrieved.</param>
        /// <param name="totalCount">Total Count of items available.</param>
        /// <param name="limit">Limit used.</param>
        /// <param name="offset">Offset used.</param>
        [SetsRequiredMembers]
        public PaginatedResponse(List<T> items, int totalCount, int limit, int offset)
        {
            this.Items = items;
            this.TotalCount = totalCount;
            this.Limit = limit;
            this.Offset = offset;
        }

        /// <summary>
        /// Items.
        /// </summary>
        required public List<T> Items { get; init; }

        /// <summary>
        /// Total Counts.
        /// </summary>
        required public int TotalCount { get; init; }

        /// <summary>
        /// Limit to Query.
        /// </summary>
        required public int Limit { get; init; }

        /// <summary>
        /// Offset to Query.
        /// </summary>
        required public int Offset { get; init; }

        /// <summary>
        /// Indicates whether there is a next page.
        /// </summary>
        public bool HasNext => this.Offset + this.Limit < this.TotalCount;

        /// <summary>
        /// Indicates whether there is a previous page.
        /// </summary>
        public bool HasPrevious => this.Offset > 0;
    }
}
