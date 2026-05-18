using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameEmgencyToEmergencyValueLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmgencyValueLimit",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "EmergencyValueLimit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EmergencyValueLimit",
                schema: "Inventory",
                table: "ItemCategory",
                newName: "EmgencyValueLimit");
        }
    }
}
