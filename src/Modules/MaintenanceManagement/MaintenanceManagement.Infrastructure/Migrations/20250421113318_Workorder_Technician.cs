using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_Technician : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "HoursSpent",
                schema: "Maintenance",
                table: "WorkOrderTechnician",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<int>(
                name: "MinutesSpent",
                schema: "Maintenance",
                table: "WorkOrderTechnician",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinutesSpent",
                schema: "Maintenance",
                table: "WorkOrderTechnician");

            migrationBuilder.AlterColumn<decimal>(
                name: "HoursSpent",
                schema: "Maintenance",
                table: "WorkOrderTechnician",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
