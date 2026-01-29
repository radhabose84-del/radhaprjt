using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_Lastactivitydate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MachineCode",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastActivityDate",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MachineId",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastActivityDate",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "MachineId",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.AddColumn<string>(
                name: "MachineCode",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "varchar(20)",
                nullable: true);
        }
    }
}
