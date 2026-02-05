using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Item_Variants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropIndex(
                name: "IX_ItemVariantAttribute_MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemVariantAttribute_MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemVariantAttribute_MiscMaster_MiscMasterId",
                schema: "Inventory",
                table: "ItemVariantAttribute",
                column: "MiscMasterId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
