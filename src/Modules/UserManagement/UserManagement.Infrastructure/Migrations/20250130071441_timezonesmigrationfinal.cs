using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class timezonesmigrationfinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "AppData",
                table: "TimeZones",
                type: "varchar(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldDefaultValueSql: "CONCAT('TZ-', LEFT(NEWID(), 8))");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "AppData",
                table: "TimeZones",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Offset",
                schema: "AppData",
                table: "TimeZones",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                schema: "AppData",
                table: "TimeZones");

            migrationBuilder.DropColumn(
                name: "Offset",
                schema: "AppData",
                table: "TimeZones");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                schema: "AppData",
                table: "TimeZones",
                type: "varchar(20)",
                nullable: false,
                defaultValueSql: "CONCAT('TZ-', LEFT(NEWID(), 8))",
                oldClrType: typeof(string),
                oldType: "varchar(20)");
        }
    }
}
