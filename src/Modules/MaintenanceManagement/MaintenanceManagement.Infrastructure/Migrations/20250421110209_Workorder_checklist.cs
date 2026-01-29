using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_checklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                newName: "StoreTypeId");

            migrationBuilder.AddColumn<bool>(
                name: "ISCompleted",
                schema: "Maintenance",
                table: "WorkOrderCheckList",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderItem_StoreTypeId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                column: "StoreTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrderItem_MiscMaster_StoreTypeId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                column: "StoreTypeId",
                principalSchema: "Maintenance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderItem_MiscMaster_StoreTypeId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrderItem_StoreTypeId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropColumn(
                name: "ISCompleted",
                schema: "Maintenance",
                table: "WorkOrderCheckList");

            migrationBuilder.RenameColumn(
                name: "StoreTypeId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                newName: "DepartmentId");
        }
    }
}
