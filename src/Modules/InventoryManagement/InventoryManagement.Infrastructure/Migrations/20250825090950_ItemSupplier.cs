using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemSupplier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DefaultSupplier",
                schema: "Inventory",
                table: "ItemSupplier",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeadTime",
                schema: "Inventory",
                table: "ItemSupplier",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MOQ",
                schema: "Inventory",
                table: "ItemSupplier",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MOQUomId",
                schema: "Inventory",
                table: "ItemSupplier",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackageUomId",
                schema: "Inventory",
                table: "ItemSupplier",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PackageValue",
                schema: "Inventory",
                table: "ItemSupplier",
                type: "decimal(18,3)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultSupplier",
                schema: "Inventory",
                table: "ItemSupplier");

            migrationBuilder.DropColumn(
                name: "LeadTime",
                schema: "Inventory",
                table: "ItemSupplier");

            migrationBuilder.DropColumn(
                name: "MOQ",
                schema: "Inventory",
                table: "ItemSupplier");

            migrationBuilder.DropColumn(
                name: "MOQUomId",
                schema: "Inventory",
                table: "ItemSupplier");

            migrationBuilder.DropColumn(
                name: "PackageUomId",
                schema: "Inventory",
                table: "ItemSupplier");

            migrationBuilder.DropColumn(
                name: "PackageValue",
                schema: "Inventory",
                table: "ItemSupplier");
        }
    }
}
