using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveFieldsFromPurchaseToInventoryAndMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginCountryId",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.DropColumn(
                name: "PurchaseRate",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.DropColumn(
                name: "SafetyStock",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.DropColumn(
                name: "TariffNumber",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.AddColumn<int>(
                name: "OriginCountryId",
                schema: "Inventory",
                table: "ItemMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TariffNumber",
                schema: "Inventory",
                table: "ItemMaster",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SafetyStock",
                schema: "Inventory",
                table: "ItemInventory",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginCountryId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropColumn(
                name: "TariffNumber",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropColumn(
                name: "SafetyStock",
                schema: "Inventory",
                table: "ItemInventory");

            migrationBuilder.AddColumn<int>(
                name: "OriginCountryId",
                schema: "Inventory",
                table: "ItemPurchase",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseRate",
                schema: "Inventory",
                table: "ItemPurchase",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SafetyStock",
                schema: "Inventory",
                table: "ItemPurchase",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TariffNumber",
                schema: "Inventory",
                table: "ItemPurchase",
                type: "varchar(50)",
                nullable: true);
        }
    }
}
