using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemCategoryEmergencyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyPoApplicable",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.RenameColumn(
                name: "EmergencyPoLimit",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "EmgencyValueLimit");

            migrationBuilder.AddColumn<int>(
                name: "EmergencyActionId",
                schema: "Inventory",
                table: "ItemCategory",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmergencyPOById",
                schema: "Inventory",
                table: "ItemCategory",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyActionId",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.DropColumn(
                name: "EmergencyPOById",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.RenameColumn(
                name: "EmgencyValueLimit",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "EmergencyPoLimit");

            migrationBuilder.AddColumn<bool>(
                name: "EmergencyPoApplicable",
                schema: "Inventory",
                table: "ItemCategory",
                type: "bit",
                nullable: true);
        }
    }
}
