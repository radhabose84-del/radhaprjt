using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_Activity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TechnicianName",
                schema: "Maintenance",
                table: "WorkOrderTechnician",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DownTimeStartTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DownTimeEndTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "Maintenance",
                table: "WorkOrderActivity",
                type: "varchar(250)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedTime",
                schema: "Maintenance",
                table: "WorkOrderActivity",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                schema: "Maintenance",
                table: "WorkOrderActivity");

            migrationBuilder.DropColumn(
                name: "EstimatedTime",
                schema: "Maintenance",
                table: "WorkOrderActivity");

            migrationBuilder.AlterColumn<string>(
                name: "TechnicianName",
                schema: "Maintenance",
                table: "WorkOrderTechnician",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DownTimeStartTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "DownTimeEndTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0),
                oldClrType: typeof(TimeSpan),
                oldType: "time",
                oldNullable: true);
        }
    }
}
