using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_oldVendor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldVendorId",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "nvarchar(20)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OldVendorId",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.AlterColumn<string>(
                name: "VendorId",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "nvarchar(20)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
