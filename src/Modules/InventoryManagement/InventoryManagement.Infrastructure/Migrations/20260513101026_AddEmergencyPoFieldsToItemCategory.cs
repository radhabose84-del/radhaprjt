using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyPoFieldsToItemCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmergencyPoApplicable",
                schema: "Inventory",
                table: "ItemCategory",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EmergencyPoLimit",
                schema: "Inventory",
                table: "ItemCategory",
                type: "decimal(18,2)",
                precision: 18,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyPoApplicable",
                schema: "Inventory",
                table: "ItemCategory");

            migrationBuilder.DropColumn(
                name: "EmergencyPoLimit",
                schema: "Inventory",
                table: "ItemCategory");
        }
    }
}
