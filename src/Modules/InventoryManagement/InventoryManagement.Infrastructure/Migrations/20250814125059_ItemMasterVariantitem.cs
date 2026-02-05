using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMasterVariantitem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantValue_NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "NewItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_NewItem_ItemMaster",
                schema: "Inventory",
                table: "ItemVariantValue",
                column: "NewItemId",
                principalSchema: "Inventory",
                principalTable: "ItemMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_NewItem_ItemMaster",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantValue_NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.DropColumn(
                name: "NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue");
        }
    }
}
