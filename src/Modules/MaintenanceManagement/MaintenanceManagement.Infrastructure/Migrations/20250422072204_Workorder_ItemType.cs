using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_ItemType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderItem_MiscMaster_MiscSourceId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderItem_MiscSourceId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropColumn(
                name: "MiscSourceId",
                schema: "Maintenance",
                table: "WorkOrderItem");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "MiscSourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItem_MiscSourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                column: "MiscSourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderItem_MiscMaster_MiscSourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                column: "MiscSourceId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
