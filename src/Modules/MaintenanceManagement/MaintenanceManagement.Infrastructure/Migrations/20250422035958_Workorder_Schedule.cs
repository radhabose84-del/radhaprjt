using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_Schedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EndTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                type: "DateTimeOffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "DateTimeOffset");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EndTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                type: "DateTimeOffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "DateTimeOffset",
                oldNullable: true);
        }
    }
}
