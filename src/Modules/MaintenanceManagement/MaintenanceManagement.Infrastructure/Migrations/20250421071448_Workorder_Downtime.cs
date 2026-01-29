using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_Downtime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.RenameColumn(
                name: "RepairStartTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "RepairEndTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                newName: "EndTime");

            migrationBuilder.AddColumn<int>(
                name: "ScarpQty",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToSubStoreQty",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "varchar(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "varchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DowntimeEnd",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "DateTimeOffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DowntimeStart",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "DateTimeOffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScarpQty",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropColumn(
                name: "ToSubStoreQty",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropColumn(
                name: "DowntimeEnd",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "DowntimeStart",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                newName: "RepairStartTime");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                newName: "RepairEndTime");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
