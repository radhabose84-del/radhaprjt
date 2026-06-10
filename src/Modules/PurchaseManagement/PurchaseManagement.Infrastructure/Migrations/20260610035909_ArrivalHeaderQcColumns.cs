using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArrivalHeaderQcColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsQcApproved",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "QcAcceptedQuantity",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QcApprovedIp",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "QcDate",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QcPersonName",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QcRejectedQuantity",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QcRejectedRemarks",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QcRemarks",
                schema: "Purchase",
                table: "ArrivalHeader",
                type: "varchar(500)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsQcApproved",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "QcAcceptedQuantity",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "QcApprovedIp",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "QcDate",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "QcPersonName",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "QcRejectedQuantity",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "QcRejectedRemarks",
                schema: "Purchase",
                table: "ArrivalHeader");

            migrationBuilder.DropColumn(
                name: "QcRemarks",
                schema: "Purchase",
                table: "ArrivalHeader");
        }
    }
}
