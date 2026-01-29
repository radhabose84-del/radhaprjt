using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class preventiveScheduleDetailcolumnRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "StatusId",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail");

            migrationBuilder.RenameColumn(
                name: "WorkOrderCreationNextDueDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "ActualWorkOrderDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActualWorkOrderDate",
                schema: "Maintenance",
                table: "PreventiveSchedulerDetail",
                newName: "WorkOrderCreationNextDueDate");

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
    }
}
