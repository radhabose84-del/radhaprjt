using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArrivalStockLedgerDropAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Purchase",
                table: "StockLedgerRaw");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                schema: "Purchase",
                table: "StockLedgerRaw");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "Purchase",
                table: "StockLedgerRaw");

            migrationBuilder.DropColumn(
                name: "CreatedIP",
                schema: "Purchase",
                table: "StockLedgerRaw");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Purchase",
                table: "StockLedgerRaw");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Purchase",
                table: "StockLedgerRaw");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Purchase",
                table: "StockLedgerRaw");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                schema: "Purchase",
                table: "StockLedgerRaw");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                schema: "Purchase",
                table: "StockLedgerRaw");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                schema: "Purchase",
                table: "StockLedgerRaw");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedIP",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "varchar(20)",
                nullable: true);
        }
    }
}
