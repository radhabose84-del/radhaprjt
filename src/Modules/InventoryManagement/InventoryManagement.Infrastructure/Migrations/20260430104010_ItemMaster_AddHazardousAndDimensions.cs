using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMaster_AddHazardousAndDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHazardous",
                schema: "Inventory",
                table: "ItemMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Breadth",
                schema: "Inventory",
                table: "ItemInventory",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DimensionUomId",
                schema: "Inventory",
                table: "ItemInventory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                schema: "Inventory",
                table: "ItemInventory",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Length",
                schema: "Inventory",
                table: "ItemInventory",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                schema: "Inventory",
                table: "ItemInventory",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemInventory_DimensionUomId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "DimensionUomId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemInventory_UOM_DimensionUomId",
                schema: "Inventory",
                table: "ItemInventory",
                column: "DimensionUomId",
                principalSchema: "Inventory",
                principalTable: "UOM",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemInventory_UOM_DimensionUomId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropIndex(
                name: "IX_ItemInventory_DimensionUomId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropColumn(
                name: "IsHazardous",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropColumn(
                name: "Breadth",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropColumn(
                name: "DimensionUomId",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropColumn(
                name: "Height",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropColumn(
                name: "Length",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.DropColumn(
                name: "Volume",
                schema: "Inventory",
                table: "ItemInventory");
        }
    }
}
