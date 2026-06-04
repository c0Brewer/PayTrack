// <copyright file="20260509153547_AddFinancePaidAtToTransaction.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancePaidAtToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<System.DateTime>(
                name: "FinancePaidAt",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinancePaidAt",
                table: "Transactions");
        }
    }
}
