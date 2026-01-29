using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Assetsubcategoriesmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropForeignKey(
                name: "FK_DepreciationGroup_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "DepreciationGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DepreciationGroup",
                schema: "FixedAsset",
                table: "DepreciationGroup");

            migrationBuilder.RenameTable(
                name: "DepreciationGroup",
                schema: "FixedAsset",
                newName: "DepreciationGroups",
                newSchema: "FixedAsset");



            migrationBuilder.RenameIndex(
                name: "IX_DepreciationGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                newName: "IX_DepreciationGroups_AssetGroupId");



            migrationBuilder.AddPrimaryKey(
                name: "PK_DepreciationGroups",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AssetSubCategories",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(10)", nullable: false),
                    SubCategoryName = table.Column<string>(type: "varchar(50)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    AssetCategoriesId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetSubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetSubCategories_AssetCategories_AssetCategoriesId",
                        column: x => x.AssetCategoriesId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetCategories_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetCategories",
                column: "AssetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetSubCategories_AssetCategoriesId",
                schema: "FixedAsset",
                table: "AssetSubCategories",
                column: "AssetCategoriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetCategories_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetCategories",
                column: "AssetGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepreciationGroups_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "AssetGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropForeignKey(
                name: "FK_DepreciationGroups_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "DepreciationGroups");


            migrationBuilder.DropTable(
                name: "AssetSubCategories",
                schema: "FixedAsset");

            migrationBuilder.DropIndex(
                name: "IX_AssetCategories_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetCategories");


            migrationBuilder.DropPrimaryKey(
                name: "PK_DepreciationGroups",
                schema: "FixedAsset",
                table: "DepreciationGroups");



            migrationBuilder.RenameTable(
                name: "DepreciationGroups",
                schema: "FixedAsset",
                newName: "DepreciationGroup",
                newSchema: "FixedAsset");

            migrationBuilder.RenameIndex(
                name: "IX_DepreciationGroups_AssetGroupId",
                schema: "FixedAsset",
                table: "DepreciationGroup",
                newName: "IX_DepreciationGroup_AssetGroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DepreciationGroup",
                schema: "FixedAsset",
                table: "DepreciationGroup",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_DepreciationGroup_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "DepreciationGroup",
                column: "AssetGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }
    }
}
