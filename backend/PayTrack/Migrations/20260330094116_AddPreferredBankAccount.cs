// <copyright file="20260330094116_AddPreferredBankAccount.cs" company="PayTrack">
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
            migrationBuilder.Sql(
                "ALTER TABLE \"User\" ADD COLUMN IF NOT EXISTS \"PreferredBankAccountId\" integer;");

            migrationBuilder.Sql(
                "ALTER TABLE \"User\" DROP CONSTRAINT IF EXISTS \"FK_User_BankAccounts_PreferredBankAccountId\";");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS \"IX_User_PreferredBankAccountId\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"User\" DROP COLUMN IF EXISTS \"PreferredBankAccountId\";");
        }
    }
}
