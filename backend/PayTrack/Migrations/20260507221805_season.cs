// <copyright file="20260507221805_season.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PayTrack.Migrations
{
    /// <inheritdoc />
    public partial class Season : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CostCentres_CostCentreId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CostCentreId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_TeamId_CostCentreId_PeriodStart_PeriodEnd",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "CostCentreId",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "BudgetId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Budgets",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Budgets",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "Budgets",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Seasons",
                columns: ["Id", "Name"],
                values: [1, "Default"]);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_BudgetId",
                table: "Transactions",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_SeasonId",
                table: "Budgets",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_PeriodStart_PeriodEnd",
                table: "Budgets",
                columns: ["TeamId", "CostCentreId", "SeasonId", "PeriodStart", "PeriodEnd"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_Name",
                table: "Seasons",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Seasons_SeasonId",
                table: "Budgets",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Budgets_BudgetId",
                table: "Transactions",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Seasons_SeasonId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Budgets_BudgetId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_BudgetId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_SeasonId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_TeamId_CostCentreId_SeasonId_PeriodStart_PeriodEnd",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "BudgetId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "Budgets");

            migrationBuilder.AddColumn<int>(
                name: "CostCentreId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CostCentreId",
                table: "Transactions",
                column: "CostCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_TeamId_CostCentreId_PeriodStart_PeriodEnd",
                table: "Budgets",
                columns: ["TeamId", "CostCentreId", "PeriodStart", "PeriodEnd"],
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CostCentres_CostCentreId",
                table: "Transactions",
                column: "CostCentreId",
                principalTable: "CostCentres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
