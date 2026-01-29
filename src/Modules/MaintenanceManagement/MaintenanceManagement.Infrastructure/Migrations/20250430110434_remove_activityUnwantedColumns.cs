using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class remove_activityUnwantedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity");

            migrationBuilder.DropColumn(
                name: "EstimatedTimeHrs",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity");

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ISCompleted",
                schema: "Maintenance",
                table: "WorkOrderSchedule");

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                type: "varchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedTimeHrs",
                schema: "Maintenance",
                table: "PreventiveSchedulerActivity",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
