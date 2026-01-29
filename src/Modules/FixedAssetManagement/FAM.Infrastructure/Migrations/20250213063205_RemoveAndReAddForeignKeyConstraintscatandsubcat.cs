using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAndReAddForeignKeyConstraintscatandsubcat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
    // Step 1: Remove existing Foreign Key constraints
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

    // Step 2: Re-add Foreign Key constraints with necessary constraints
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
             // Remove Foreign Keys
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

    // Re-add Foreign Key constraints in case of rollback
    migrationBuilder.AddForeignKey(
        name: "FK_AssetCategories_AssetGroup",
        schema: "FixedAsset",
        table: "AssetCategories",
        column: "AssetGroupId",
        principalSchema: "FixedAsset",
        principalTable: "AssetGroup",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict
    );

    migrationBuilder.AddForeignKey(
        name: "FK_AssetSubCategories_AssetCategories",
        schema: "FixedAsset",
        table: "AssetSubCategories",
        column: "AssetCategoriesId",
        principalSchema: "FixedAsset",
        principalTable: "AssetCategories",
        principalColumn: "Id",
        onDelete: ReferentialAction.Restrict
    );
        }
    }
}
