using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class constraintmigrationasscatandsubcat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           // Add Foreign Key for AssetGroup in AssetCategories
        migrationBuilder.AddForeignKey(
        name: "FK_AssetCategories_AssetGroup",
        schema: "FixedAsset",
        table: "AssetCategories",
        column: "AssetGroupId",
        principalSchema: "FixedAsset",
        principalTable: "AssetGroup",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict // Prevent cascading delete if needed
    );

    // Add Foreign Key for AssetCategories in AssetSubCategories
    migrationBuilder.AddForeignKey(
        name: "FK_AssetSubCategories_AssetCategories",
        schema: "FixedAsset",
        table: "AssetSubCategories",
        column: "AssetCategoriesId",
        principalSchema: "FixedAsset",
        principalTable: "AssetCategories",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict // Prevent cascading delete if needed
    );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                migrationBuilder.DropForeignKey(
                name: "FK_AssetCategories_AssetGroup",
                schema: "FixedAsset",
                table: "AssetCategories"
    );

                migrationBuilder.DropForeignKey(
                name: "FK_AssetSubCategories_AssetCategories",
                schema: "FixedAsset",
                table: "AssetSubCategories"
    );
        }
    }
}
