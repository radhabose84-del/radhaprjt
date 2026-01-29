using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_new_Request : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkOrder_RequestId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrder_MaintenanceRequest_RequestId",
                schema: "Maintenance",
                table: "WorkOrder",
                column: "RequestId",
                principalSchema: "Maintenance",
                principalTable: "MaintenanceRequest",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrder_MaintenanceRequest_RequestId",
                schema: "Maintenance",
                table: "WorkOrder");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrder_RequestId",
                schema: "Maintenance",
                table: "WorkOrder");
        }
    }
}
