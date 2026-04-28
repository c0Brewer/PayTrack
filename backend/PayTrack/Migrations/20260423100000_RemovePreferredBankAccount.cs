// <copyright file="20260423100000_RemovePreferredBankAccount.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PayTrack.Data;

#nullable disable

namespace PayTrack.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260423100000_RemovePreferredBankAccount")]
    public partial class RemovePreferredBankAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"User\" DROP CONSTRAINT IF EXISTS \"FK_User_BankAccounts_PreferredBankAccountId\";");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS \"IX_User_PreferredBankAccountId\";");

            migrationBuilder.Sql(
                "ALTER TABLE \"User\" DROP COLUMN IF EXISTS \"PreferredBankAccountId\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "ALTER TABLE \"User\" ADD COLUMN IF NOT EXISTS \"PreferredBankAccountId\" integer;");
        }
    }
}
