// <copyright file="20260604070814_AddCreditorNameToPaymentRequestByUser.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditorNameToPaymentRequestByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreditorName",
                table: "Transactions",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditorName",
                table: "Transactions");
        }
    }
}
