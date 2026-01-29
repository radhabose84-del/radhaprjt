using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class preventivelogColumnAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PreventiveScheduleDetailId",
                schema: "Maintenance",
                table: "PreventiveScheduleLog",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PreventiveScheduleId",
                schema: "Maintenance",
                table: "PreventiveScheduleLog",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreventiveScheduleId",
                schema: "Maintenance",
                table: "PreventiveScheduleLog");

            migrationBuilder.AlterColumn<int>(
                name: "PreventiveScheduleDetailId",
                schema: "Maintenance",
                table: "PreventiveScheduleLog",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
