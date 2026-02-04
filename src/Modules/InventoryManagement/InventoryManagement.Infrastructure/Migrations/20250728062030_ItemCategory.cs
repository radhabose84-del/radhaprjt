using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategory_ItemGroup_GroupId",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "ItemGroupId");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "ItemCategoryName");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCategory_GroupId",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "IX_ItemCategory_ItemGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategory_ItemGroup_ItemGroupId",
                schema: "Inventory",
                table: "ItemCategory",
                column: "ItemGroupId",
                principalSchema: "Inventory",
                principalTable: "ItemGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemCategory_ItemGroup_ItemGroupId",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.RenameColumn(
                name: "ItemGroupId",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "GroupId");

            migrationBuilder.RenameColumn(
                name: "ItemCategoryName",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "CategoryName");

            migrationBuilder.RenameIndex(
                name: "IX_ItemCategory_ItemGroupId",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "IX_ItemCategory_GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemCategory_ItemGroup_GroupId",
                schema: "Inventory",
                table: "ItemCategory",
                column: "GroupId",
                principalSchema: "Inventory",
                principalTable: "ItemGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
