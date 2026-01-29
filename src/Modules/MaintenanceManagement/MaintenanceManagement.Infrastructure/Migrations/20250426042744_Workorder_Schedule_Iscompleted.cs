using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_Schedule_Iscompleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ISCompleted",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.AddColumn<bool>(
                name: "ISCompleted",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ISCompleted",
                schema: "Maintenance",
                table: "WorkOrderSchedule");

            migrationBuilder.AddColumn<bool>(
                name: "ISCompleted",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "bit",
                nullable: true);
        }
    }
}
