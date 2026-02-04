using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemPurchaseSourceofItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemMaster_MiscMaster_SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropIndex(
                name: "IX_ItemMaster_SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropColumn(
                name: "SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.AddColumn<int>(
                name: "SourceOfItem",
                schema: "Inventory",
                table: "ItemPurchase",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPurchase_SourceOfItem",
                schema: "Inventory",
                table: "ItemPurchase",
                column: "SourceOfItem");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemPurchase_MiscMaster_SourceOfItem",
                schema: "Inventory",
                table: "ItemPurchase",
                column: "SourceOfItem",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemPurchase_MiscMaster_SourceOfItem",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.DropIndex(
                name: "IX_ItemPurchase_SourceOfItem",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.DropColumn(
                name: "SourceOfItem",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.AddColumn<int>(
                name: "SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "SourceOfItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMaster_MiscMaster_SourceOfItemId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "SourceOfItemId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
