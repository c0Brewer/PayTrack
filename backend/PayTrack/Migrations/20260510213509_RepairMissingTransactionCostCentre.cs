// <copyright file="20260510213509_RepairMissingTransactionCostCentre.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayTrack.Migrations
{
    /// <inheritdoc />
    public partial class RepairMissingTransactionCostCentre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'Transactions'
                          AND column_name = 'CostCentreId'
                    ) THEN
                        ALTER TABLE "Transactions" ADD COLUMN "CostCentreId" integer NULL;
                    END IF;

                    ALTER TABLE "Transactions" ALTER COLUMN "CostCentreId" DROP NOT NULL;
                END $$;
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_Transactions_CostCentreId"
                ON "Transactions" ("CostCentreId");
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'FK_Transactions_CostCentres_CostCentreId'
                    ) THEN
                        ALTER TABLE "Transactions"
                        ADD CONSTRAINT "FK_Transactions_CostCentres_CostCentreId"
                        FOREIGN KEY ("CostCentreId")
                        REFERENCES "CostCentres" ("Id")
                        ON DELETE RESTRICT
                        NOT VALID;
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
