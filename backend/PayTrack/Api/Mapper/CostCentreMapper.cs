// <copyright file="CostCentreMapper.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using PayTrack.Application.Dto.CostCentre;
using PayTrack.Data.Entities;

namespace PayTrack.Api.Mapper
{
    /// <summary>
    /// Mapper for CostCentre.
    /// </summary>
    public static class CostCentreMapper
    {
        /// <summary>
        /// Turns CostCentre object into a CostCentreDto.
        /// </summary>
        /// <param name="costCentre">CostCentre to map.</param>
        /// <returns>CostCentreDto instance.</returns>
        public static CostCentreDto ToDto(CostCentre costCentre)
        {
            return new CostCentreDto(
                costCentre.Id,
                costCentre.Name,
                costCentre.Description,
                costCentre.DisplayColor);
        }

        /// <summary>
        /// Turns a List of CostCentre objects into a List of CostCentreDto objects.
        /// </summary>
        /// <param name="costCentres">List of CostCentre objects.</param>
        /// <returns>List of CostCentreDto objects.</returns>
        public static List<CostCentreDto> ListToDto(List<CostCentre> costCentres)
        {
            return costCentres.ConvertAll(ToDto);
        }
    }
}
