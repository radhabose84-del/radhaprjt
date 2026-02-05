using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHSNMastercoulmnrename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SgstPercentage",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "SGSTPercentage");

            migrationBuilder.RenameColumn(
                name: "IgstPercentage",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "IGSTPercentage");

            migrationBuilder.RenameColumn(
                name: "GstPercentage",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "GSTPercentage");

            migrationBuilder.RenameColumn(
                name: "CgstPercentage",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "CGSTPercentage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SGSTPercentage",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "SgstPercentage");

            migrationBuilder.RenameColumn(
                name: "IGSTPercentage",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "IgstPercentage");

            migrationBuilder.RenameColumn(
                name: "GSTPercentage",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "GstPercentage");

            migrationBuilder.RenameColumn(
                name: "CGSTPercentage",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "CgstPercentage");
        }
    }
}
