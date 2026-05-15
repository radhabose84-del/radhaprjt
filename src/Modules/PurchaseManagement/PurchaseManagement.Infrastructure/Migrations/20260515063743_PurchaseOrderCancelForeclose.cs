using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseOrderCancelForeclose : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancelledByName",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledDate",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledIP",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ForeClosedByName",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ForeClosedDate",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ForeClosedIP",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledByName",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "CancelledDate",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "CancelledIP",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "ForeClosedByName",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "ForeClosedDate",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "ForeClosedIP",
                schema: "Purchase",
                table: "PurchaseOrderHeader");
        }
    }
}
