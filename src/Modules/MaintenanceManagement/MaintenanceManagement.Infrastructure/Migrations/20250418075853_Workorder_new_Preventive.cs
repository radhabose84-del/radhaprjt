using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_new_Preventive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_PreventiveScheduleId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "PreventiveScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_PreventiveSchedulerDetail_PreventiveScheduleId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "PreventiveScheduleId",
                principalSchema: "Maintenance",
                principalTable: "PreventiveSchedulerDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_PreventiveSchedulerDetail_PreventiveScheduleId",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_PreventiveScheduleId",
                schema: "Maintenance",
                table: "WorkOrder");
        }
    }
}
