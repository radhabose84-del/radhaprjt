using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class assetdisposaltypeonetomany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssetDisposal_DisposalType",
                schema: "FixedAsset",
                table: "AssetDisposal");

            migrationBuilder.CreateIndex(
                name: "IX_AssetDisposal_DisposalType",
                schema: "FixedAsset",
                table: "AssetDisposal",
                column: "DisposalType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AssetDisposal_DisposalType",
                schema: "FixedAsset",
                table: "AssetDisposal");

            migrationBuilder.CreateIndex(
                name: "IX_AssetDisposal_DisposalType",
                schema: "FixedAsset",
                table: "AssetDisposal",
                column: "DisposalType",
                unique: true);
        }
    }
}
