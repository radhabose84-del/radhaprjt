using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_ItemSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderItem_MiscMaster_SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderItem_SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropColumn(
                name: "SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItem_SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderItem_MiscMaster_SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                column: "SourceId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
