using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_Itemnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderItem_MiscMaster_SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                newName: "MiscSourceId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderItem_SourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                newName: "IX_WorkOrderItem_MiscSourceId");

            migrationBuilder.AlterColumn<int>(
                name: "StoreTypeId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrderItem_MiscMaster_MiscSourceId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.RenameColumn(
                name: "MiscSourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                newName: "SourceId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrderItem_MiscSourceId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                newName: "IX_WorkOrderItem_SourceId");

            migrationBuilder.AlterColumn<int>(
                name: "StoreTypeId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
