using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetDepreciationGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepreciationGroups_AssetGroupId",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.AlterColumn<int>(
                name: "DepreciationMethod",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BookType",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationGroups_AssetGroupId_DepreciationMethod_BookType_DepreciationGroupName_IsActive",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                columns: new[] { "AssetGroupId", "DepreciationMethod", "BookType", "DepreciationGroupName", "IsActive" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepreciationGroups_AssetGroupId_DepreciationMethod_BookType_DepreciationGroupName_IsActive",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.AlterColumn<int>(
                name: "DepreciationMethod",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BookType",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationGroups_AssetGroupId",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "AssetGroupId");
        }
    }
}
