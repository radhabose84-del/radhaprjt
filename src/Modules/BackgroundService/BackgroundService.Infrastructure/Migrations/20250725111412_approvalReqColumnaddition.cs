using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalReqColumnaddition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "varchar(255)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "AppData",
                table: "ApprovalRequest");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                schema: "AppData",
                table: "ApprovalRequest");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                schema: "AppData",
                table: "ApprovalRequest");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                schema: "AppData",
                table: "ApprovalRequest");
        }
    }
}
