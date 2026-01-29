using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class currenytimeIsdeletemigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "AppData",
                table: "TimeZones",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "AppData",
                table: "Currency",
                newName: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "TimeZones",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                schema: "AppData",
                table: "Currency",
                newName: "Status");
        }
    }
}
