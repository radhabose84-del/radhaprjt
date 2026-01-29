using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class preventiveDescriptioncolumnalter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OldCategoryDescription",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                type: "varchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OldCategoryDescription",
                schema: "Maintenance",
                table: "PreventiveSchedulerItems",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(max)",
                oldNullable: true);
        }
    }
}
