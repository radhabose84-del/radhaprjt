using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAssetDisposalMasterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_AssetDisposal_AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster"
            );

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster"
            );

            migrationBuilder.DropColumn(
                name: "AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                migrationBuilder.AddColumn<int>(
                name: "AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true
            );

                migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetDisposalMasterId"
            );

                migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_AssetDisposal_AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetDisposalMasterId",
                principalSchema: "FixedAsset",
                principalTable: "AssetDisposal",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict
            );
        }
    }
}
