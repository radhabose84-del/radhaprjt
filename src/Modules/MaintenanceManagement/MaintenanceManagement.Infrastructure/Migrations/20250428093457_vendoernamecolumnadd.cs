using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class vendoernamecolumnadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OldVendorName",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorName",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OldVendorName",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.DropColumn(
                name: "VendorName",
                schema: "Maintenance",
                table: "MaintenanceRequest");
        }
    }
}
