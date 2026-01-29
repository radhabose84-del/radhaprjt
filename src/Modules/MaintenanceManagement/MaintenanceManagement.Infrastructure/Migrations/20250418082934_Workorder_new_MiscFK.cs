using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_new_MiscFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_StatusId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_MiscMaster_StatusId",
                schema: "Maintenance",
                table: "WorkOrder",
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
                name: "FK_WorkOrder_MiscMaster_StatusId",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_StatusId",
                schema: "Maintenance",
                table: "WorkOrder");
        }
    }
}
