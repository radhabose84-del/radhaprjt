using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetDepreciation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepreciationGroups_AssetGroupId_DepreciationMethod_BookType_DepreciationGroupName_IsActive",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationGroups_AssetGroupId_DepreciationMethod_BookType_IsActive",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                columns: new[] { "AssetGroupId", "DepreciationMethod", "BookType", "IsActive" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepreciationGroups_AssetGroupId_DepreciationMethod_BookType_IsActive",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationGroups_AssetGroupId_DepreciationMethod_BookType_DepreciationGroupName_IsActive",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                columns: new[] { "AssetGroupId", "DepreciationMethod", "BookType", "DepreciationGroupName", "IsActive" },
                unique: true);
        }
    }
}
