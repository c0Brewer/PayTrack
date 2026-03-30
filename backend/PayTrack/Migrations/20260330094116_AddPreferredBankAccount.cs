// <copyright file="20260330094116_AddPreferredBankAccount:.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferredBankAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PreferredBankAccountId",
                table: "User",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_PreferredBankAccountId",
                table: "User",
                column: "PreferredBankAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_BankAccounts_PreferredBankAccountId",
                table: "User",
                column: "PreferredBankAccountId",
                principalTable: "BankAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_BankAccounts_PreferredBankAccountId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_PreferredBankAccountId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PreferredBankAccountId",
                table: "User");
        }
    }
}
