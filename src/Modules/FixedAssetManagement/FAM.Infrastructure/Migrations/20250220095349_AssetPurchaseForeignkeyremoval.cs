using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetPurchaseForeignkeyremoval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetPurchaseDetails_UOM_UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");

            migrationBuilder.DropIndex(
                name: "IX_AssetPurchaseDetails_UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");

            migrationBuilder.DropColumn(
                name: "UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetPurchaseDetails_UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "UOMId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetPurchaseDetails_UOM_UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "UOMId",
                principalSchema: "FixedAsset",
                principalTable: "UOM",
                principalColumn: "Id");
        }
    }
}
