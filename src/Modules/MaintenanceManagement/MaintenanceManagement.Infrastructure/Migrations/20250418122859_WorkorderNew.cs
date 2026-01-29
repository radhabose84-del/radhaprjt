using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WorkorderNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "varchar(250)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)");

            migrationBuilder.AlterColumn<int>(
                name: "RootCauseId",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ItemName",
                schema: "Maintenance",
                table: "WorkOrderItem",
                type: "varchar(250)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(250)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RootCauseId",
                schema: "Maintenance",
                table: "WorkOrder",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
