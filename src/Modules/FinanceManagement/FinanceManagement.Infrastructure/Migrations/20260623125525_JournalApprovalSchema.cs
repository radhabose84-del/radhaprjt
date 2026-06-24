using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class JournalApprovalSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DraftSavedAt",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "SubmittedBy",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.AlterColumn<string>(
                name: "RejectedBy",
                schema: "Finance",
                table: "JournalHeader",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PostedBy",
                schema: "Finance",
                table: "JournalHeader",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedBy",
                schema: "Finance",
                table: "JournalHeader",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RejectedBy",
                schema: "Finance",
                table: "JournalHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PostedBy",
                schema: "Finance",
                table: "JournalHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedBy",
                schema: "Finance",
                table: "JournalHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DraftSavedAt",
                schema: "Finance",
                table: "JournalHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SubmittedAt",
                schema: "Finance",
                table: "JournalHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubmittedBy",
                schema: "Finance",
                table: "JournalHeader",
                type: "int",
                nullable: true);
        }
    }
}
