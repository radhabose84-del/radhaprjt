using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSchemaForAssetSubCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             // Ensure the new schema exists
            migrationBuilder.EnsureSchema("FixedAsset");

            // Move the table from 'dbo' to 'FixedAsset'
            migrationBuilder.RenameTable(
            name: "AssetSubCategories",
            schema: "dbo",
            newName: "AssetSubCategories",
            newSchema: "FixedAsset"
    );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
              // Move back to 'dbo' if rolled back
            migrationBuilder.RenameTable(
            name: "AssetSubCategories",
            schema: "FixedAsset",
            newName: "AssetSubCategories",
            newSchema: "dbo"
    );
        }
    }
}
