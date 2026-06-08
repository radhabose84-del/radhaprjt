using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RawMaterialPOCottonFieldsAndDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArrivalType",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CottonApprovedBy",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CottonApprovedOn",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreditDays",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CropYear",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                type: "varchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PassingDate",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArrivalType",
                schema: "Purchase",
                table: "RawMaterialPOHeader");

            migrationBuilder.DropColumn(
                name: "CottonApprovedBy",
                schema: "Purchase",
                table: "RawMaterialPOHeader");

            migrationBuilder.DropColumn(
                name: "CottonApprovedOn",
                schema: "Purchase",
                table: "RawMaterialPOHeader");

            migrationBuilder.DropColumn(
                name: "CreditDays",
                schema: "Purchase",
                table: "RawMaterialPOHeader");

            migrationBuilder.DropColumn(
                name: "CropYear",
                schema: "Purchase",
                table: "RawMaterialPOHeader");

            migrationBuilder.DropColumn(
                name: "DocumentPath",
                schema: "Purchase",
                table: "RawMaterialPOHeader");

            migrationBuilder.DropColumn(
                name: "PassingDate",
                schema: "Purchase",
                table: "RawMaterialPOHeader");
        }
    }
}
