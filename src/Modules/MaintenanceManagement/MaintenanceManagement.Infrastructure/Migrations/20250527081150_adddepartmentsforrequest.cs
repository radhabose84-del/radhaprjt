using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class adddepartmentsforrequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                newName: "ProductionDepartmentId");

            migrationBuilder.AddColumn<int>(
                name: "MaintenanceDepartmentId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaintenanceDepartmentId",
                schema: "Maintenance",
                table: "MaintenanceRequest");

            migrationBuilder.RenameColumn(
                name: "ProductionDepartmentId",
                schema: "Maintenance",
                table: "MaintenanceRequest",
                newName: "DepartmentId");
        }
    }
}
