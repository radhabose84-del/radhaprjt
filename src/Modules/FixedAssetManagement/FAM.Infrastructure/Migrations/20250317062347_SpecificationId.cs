using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SpecificationId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetSpecifications_Manufacture_ManufactureId",
                schema: "FixedAsset",
                table: "AssetSpecifications");

            migrationBuilder.DropIndex(
                name: "IX_AssetSpecifications_ManufactureId",
                schema: "FixedAsset",
                table: "AssetSpecifications");

            migrationBuilder.DropColumn(
                name: "ManufactureDate",
                schema: "FixedAsset",
                table: "AssetSpecifications");

            migrationBuilder.DropColumn(
                name: "ManufactureId",
                schema: "FixedAsset",
                table: "AssetSpecifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ManufactureDate",
                schema: "FixedAsset",
                table: "AssetSpecifications",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ManufactureId",
                schema: "FixedAsset",
                table: "AssetSpecifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetSpecifications_ManufactureId",
                schema: "FixedAsset",
                table: "AssetSpecifications",
                column: "ManufactureId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetSpecifications_Manufacture_ManufactureId",
                schema: "FixedAsset",
                table: "AssetSpecifications",
                column: "ManufactureId",
                principalSchema: "FixedAsset",
                principalTable: "Manufacture",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
