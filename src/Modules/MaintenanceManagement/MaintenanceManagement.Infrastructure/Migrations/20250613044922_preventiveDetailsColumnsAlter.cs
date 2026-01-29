using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class preventiveDetailsColumnsAlter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DownTimeEstimateHrs",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FrequencyInterval",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GraceDays",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDownTimeRequired",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReminderMaterialReqDays",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReminderWorkOrderDays",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerDetail_FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "FrequencyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerDetail_FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "FrequencyUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PreventiveSchedulerDetail_ScheduleId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerDetail_MiscMaster_FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "FrequencyTypeId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerDetail_MiscMaster_FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "FrequencyUnitId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PreventiveSchedulerDetail_MiscMaster_ScheduleId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                column: "ScheduleId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerDetail_MiscMaster_FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerDetail_MiscMaster_FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_PreventiveSchedulerDetail_MiscMaster_ScheduleId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropIndex(
                name: "IX_PreventiveSchedulerDetail_FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropIndex(
                name: "IX_PreventiveSchedulerDetail_FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropIndex(
                name: "IX_PreventiveSchedulerDetail_ScheduleId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "DownTimeEstimateHrs",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "FrequencyInterval",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "FrequencyTypeId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "FrequencyUnitId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "GraceDays",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "IsDownTimeRequired",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "ReminderMaterialReqDays",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "ReminderWorkOrderDays",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");
        }
    }
}
