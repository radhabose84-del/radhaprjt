using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class unit_company_column_addPreventive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerHeader");
        }
    }
}
