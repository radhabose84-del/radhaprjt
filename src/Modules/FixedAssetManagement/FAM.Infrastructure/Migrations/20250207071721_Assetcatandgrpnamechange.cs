using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Assetcatandgrpnamechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "FixedAsset",
                table: "AssetGroup",
                newName: "GroupName");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "FixedAsset",
                table: "AssetCategories",
                newName: "CategoryName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GroupName",
                schema: "FixedAsset",
                table: "AssetGroup",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                schema: "FixedAsset",
                table: "AssetCategories",
                newName: "Name");
        }
    }
}
