// <copyright file="20260624204940_AddNameToBudgetUniqueIndex.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToBudgetUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_Type_PeriodStart_Perio~",
                table: "Budgets");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_Type_PeriodStart_Perio~",
                table: "Budgets",
                columns: new[] { "TeamId", "CostCentreId", "SeasonId", "Type", "PeriodStart", "PeriodEnd", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_Type_PeriodStart_Perio~",
                table: "Budgets");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_Type_PeriodStart_Perio~",
                table: "Budgets",
                columns: new[] { "TeamId", "CostCentreId", "SeasonId", "Type", "PeriodStart", "PeriodEnd" },
                unique: true);
        }
    }
}
