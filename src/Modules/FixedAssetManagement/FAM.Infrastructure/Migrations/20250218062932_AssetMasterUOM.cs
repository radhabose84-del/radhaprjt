using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetMasterUOM : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure UOMId in AssetMaster can accept NULL values
            migrationBuilder.AlterColumn<int>(
                name: "UOMId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // Create index for UOMId in AssetMaster
            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_UOMId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "UOMId");

            // Add foreign key constraint to UOMId referencing UOM table
            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_UOM",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "UOMId",
                principalSchema: "FixedAsset",
                principalTable: "UOM",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_UOM",
                schema: "FixedAsset",
                table: "AssetMaster");

            // Remove the index for UOMId in AssetMaster
            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_UOMId",
                schema: "FixedAsset",
                table: "AssetMaster");

            // Restore the original column definition (if needed)
            migrationBuilder.AlterColumn<int>(
                name: "UOMId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
