using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMasterVariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChangesJson",
                schema: "Inventory",
                table: "ItemLogs");

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                schema: "Inventory",
                table: "ItemLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                schema: "Inventory",
                table: "ItemLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PropertyName",
                schema: "Inventory",
                table: "ItemLogs",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewValue",
                schema: "Inventory",
                table: "ItemLogs");

            migrationBuilder.DropColumn(
                name: "OldValue",
                schema: "Inventory",
                table: "ItemLogs");

            migrationBuilder.DropColumn(
                name: "PropertyName",
                schema: "Inventory",
                table: "ItemLogs");

            migrationBuilder.AddColumn<string>(
                name: "ChangesJson",
                schema: "Inventory",
                table: "ItemLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");
        }
    }
}
