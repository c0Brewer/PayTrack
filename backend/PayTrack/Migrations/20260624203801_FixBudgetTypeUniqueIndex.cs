// <copyright file="20260624203801_FixBudgetTypeUniqueIndex.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayTrack.Migrations
{
    /// <inheritdoc />
    public partial class FixBudgetTypeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_PeriodStart_PeriodEnd",
                table: "Budgets");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_Type_PeriodStart_Perio~",
                table: "Budgets",
                columns: new[] { "TeamId", "CostCentreId", "SeasonId", "Type", "PeriodStart", "PeriodEnd" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_Type_PeriodStart_Perio~",
                table: "Budgets");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_PeriodStart_PeriodEnd",
                table: "Budgets",
                columns: new[] { "TeamId", "CostCentreId", "SeasonId", "PeriodStart", "PeriodEnd" },
                unique: true);
        }
    }
}
