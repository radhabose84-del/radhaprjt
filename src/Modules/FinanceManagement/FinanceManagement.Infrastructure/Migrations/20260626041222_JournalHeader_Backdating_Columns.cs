using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class JournalHeader_Backdating_Columns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BackdateAcknowledgedAt",
                schema: "Finance",
                table: "JournalHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BackdateAcknowledgedBy",
                schema: "Finance",
                table: "JournalHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackdateReason",
                schema: "Finance",
                table: "JournalHeader",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBackdated",
                schema: "Finance",
                table: "JournalHeader",
                type: "bit",
                nullable: false,
                computedColumnSql: "CASE WHEN VoucherDate IS NULL OR PostedAt IS NULL THEN CAST(0 AS BIT) WHEN VoucherDate < CAST(PostedAt AS DATE) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_IsBackdated",
                schema: "Finance",
                table: "JournalHeader",
                columns: new[] { "IsBackdated", "IsDeleted", "CompanyId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalHeader_IsBackdated",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "IsBackdated",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "BackdateAcknowledgedAt",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "BackdateAcknowledgedBy",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "BackdateReason",
                schema: "Finance",
                table: "JournalHeader");
        }
    }
}
