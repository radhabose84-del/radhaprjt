using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Item_PurchaseReate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PurchaseRate",
                schema: "Inventory",
                table: "ItemPurchase",
                type: "decimal(18,3)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                schema: "Inventory",
                table: "ItemMaster",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchaseRate",
                schema: "Inventory",
                table: "ItemPurchase");

            migrationBuilder.AlterColumn<int>(
                name: "UnitId",
                schema: "Inventory",
                table: "ItemMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
