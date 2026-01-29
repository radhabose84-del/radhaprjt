using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_Schedulestatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ISCompleted",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                newName: "IsCompleted");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderSchedule_StatusId",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderSchedule_MiscMaster_StatusId",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                column: "StatusId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderSchedule_MiscMaster_StatusId",
                schema: "Maintenance",
                table: "WorkOrderSchedule");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderSchedule_StatusId",
                schema: "Maintenance",
                table: "WorkOrderSchedule");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Maintenance",
                table: "WorkOrderSchedule");           

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                schema: "Maintenance",
                table: "WorkOrderSchedule",
                newName: "ISCompleted");
        }
    }
}
