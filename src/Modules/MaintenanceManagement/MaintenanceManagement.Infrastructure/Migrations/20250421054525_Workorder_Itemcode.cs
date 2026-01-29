using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Workorder_Itemcode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropColumn(
                name: "OldItemId",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldItemCode",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "nvarchar(100)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItemCode",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.DropColumn(
                name: "OldItemCode",
                schema: "Maintenance",
                table: "WorkOrderItem");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldItemId",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "int",
                nullable: true);
        }
    }
}
