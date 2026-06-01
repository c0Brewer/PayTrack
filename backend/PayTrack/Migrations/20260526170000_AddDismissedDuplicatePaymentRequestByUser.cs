// <copyright file="20260526170000_AddDismissedDuplicatePaymentRequestByUser.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PayTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddDismissedDuplicatePaymentRequestByUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DismissedDuplicatePaymentRequestsByUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstPaymentRequestByUserId = table.Column<int>(type: "integer", nullable: false),
                    SecondPaymentRequestByUserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissedDuplicatePaymentRequestsByUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DismissedDuplicatePairs_Transactions_First",
                        column: x => x.FirstPaymentRequestByUserId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DismissedDuplicatePairs_Transactions_Second",
                        column: x => x.SecondPaymentRequestByUserId,
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DismissedDuplicatePairs_First_Second",
                table: "DismissedDuplicatePaymentRequestsByUser",
                columns: new[] { "FirstPaymentRequestByUserId", "SecondPaymentRequestByUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DismissedDuplicatePairs_Second",
                table: "DismissedDuplicatePaymentRequestsByUser",
                column: "SecondPaymentRequestByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DismissedDuplicatePaymentRequestsByUser");
        }
    }
}
