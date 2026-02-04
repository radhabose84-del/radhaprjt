using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMaster_Variant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantValue_NewItem_ItemMaster",
                schema: "Inventory",
                table: "ItemVariantValue");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantValue_ItemMaster_NewItemId",
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
                name: "FK_ItemVariantValue_ItemMaster_NewItemId",
                schema: "Inventory",
                table: "ItemVariantValue");

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
    }
}
