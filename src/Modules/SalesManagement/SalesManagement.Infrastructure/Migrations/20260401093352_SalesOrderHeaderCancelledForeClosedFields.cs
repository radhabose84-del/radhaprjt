using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderHeaderCancelledForeClosedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelledByName",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledIP",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ForeClosedByName",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ForeClosedDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ForeClosedIP",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(50)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledByName",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "CancelledDate",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "CancelledIP",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "ForeClosedByName",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "ForeClosedDate",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "ForeClosedIP",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
