using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class preventiveScheduleDetailcolumnRemove_addBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialReqNextDueDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedIP",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "varchar(255)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "CreatedIP",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.AddColumn<DateOnly>(
                name: "MaterialReqNextDueDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "date",
                nullable: true);
        }
    }
}
