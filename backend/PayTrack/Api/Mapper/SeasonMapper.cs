// <copyright file="SeasonMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.Season;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for Season.
    /// </summary>
    public static class SeasonMapper
    {
        /// <summary>
        /// Turns a Season object into a SeasonDto.
        /// </summary>
        /// <param name="season">Season to map.</param>
        /// <returns>SeasonDto instance.</returns>
        public static SeasonDto ToDto(Season season)
        {
            return new SeasonDto(
                season.Id,
                season.Name,
                BudgetMapper.CollectionToDto(season.Budgets));
        }

        /// <summary>
        /// Turns a list of Season objects into a list of SeasonDto objects.
        /// </summary>
        /// <param name="seasons">List of Season objects.</param>
        /// <returns>List of SeasonDto objects.</returns>
        public static List<SeasonDto> ListToDto(ICollection<Season> seasons)
        {
            return [.. seasons.Select(ToDto)];
        }
    }
}
