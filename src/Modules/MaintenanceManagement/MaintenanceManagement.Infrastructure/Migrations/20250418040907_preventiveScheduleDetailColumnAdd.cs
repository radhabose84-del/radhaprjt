using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class preventiveScheduleDetailColumnAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "WorkOrderCreationStartDate");

            migrationBuilder.RenameColumn(
                name: "NextDueDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "WorkOrderCreationNextDueDate");

            migrationBuilder.AddColumn<DateOnly>(
                name: "MaterialReqNextDueDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "MaterialReqStartDays",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerDetail_StatusId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerDetail_MiscMaster_StatusId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "StatusId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerDetail_MiscMaster_StatusId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropIndex(
                name: "IX_PreventiveSchedulerDetail_StatusId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "MaterialReqNextDueDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "MaterialReqStartDays",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.RenameColumn(
                name: "WorkOrderCreationStartDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "WorkOrderCreationNextDueDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "NextDueDate");
        }
    }
}
