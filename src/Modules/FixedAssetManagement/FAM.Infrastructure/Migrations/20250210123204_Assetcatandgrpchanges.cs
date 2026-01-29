using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Assetcatandgrpchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetCategories_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetCategories");

            migrationBuilder.DropIndex(
                name: "IX_AssetCategories_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetCategories");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AssetCategories_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetCategories",
                column: "AssetGroupId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetCategories_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetCategories",
                column: "AssetGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
