using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssetDisposalPurchaseFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                migrationBuilder.DropForeignKey(
                name: "FK_AssetPurchaseDetails_AssetDisposal_AssetDisposalPurchaseId",
                schema: "FixedAsset",  // Specify the schema
                table: "AssetPurchaseDetails"
            );

                migrationBuilder.DropIndex(
                name: "IX_AssetPurchaseDetails_AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails"
            );

                migrationBuilder.DropColumn(
                name: "AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

                migrationBuilder.AddColumn<int>(
                name: "AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                type: "int",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_AssetPurchaseDetails_AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "AssetDisposalPurchaseId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AssetPurchaseDetails_AssetDisposal_AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "AssetDisposalPurchaseId",
                principalSchema: "FixedAsset",
                principalTable: "AssetDisposal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );

        }
    }
}
