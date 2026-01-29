using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetPurchasemigrationnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetPurchaseDetails_AssetMaster_AssetMasterId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetPurchaseDetails_UOM_UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");

            migrationBuilder.RenameColumn(
                name: "AssetMasterId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                newName: "AssetId");

            migrationBuilder.RenameIndex(
                name: "IX_AssetPurchaseDetails_AssetMasterId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                newName: "IX_AssetPurchaseDetails_AssetId");

            migrationBuilder.AlterColumn<int>(
                name: "UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CapitalizationDate",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetPurchaseDetails_AssetMaster_AssetId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "AssetId",
                principalSchema: "FixedAsset",
                principalTable: "AssetMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetPurchaseDetails_UOM_UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "UOMId",
                principalSchema: "FixedAsset",
                principalTable: "UOM",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetPurchaseDetails_AssetMaster_AssetId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetPurchaseDetails_UOM_UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");

            migrationBuilder.RenameColumn(
                name: "AssetId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                newName: "AssetMasterId");

            migrationBuilder.RenameIndex(
                name: "IX_AssetPurchaseDetails_AssetId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                newName: "IX_AssetPurchaseDetails_AssetMasterId");

            migrationBuilder.AlterColumn<int>(
                name: "UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CapitalizationDate",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetPurchaseDetails_AssetMaster_AssetMasterId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "AssetMasterId",
                principalSchema: "FixedAsset",
                principalTable: "AssetMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetPurchaseDetails_UOM_UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "UOMId",
                principalSchema: "FixedAsset",
                principalTable: "UOM",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
