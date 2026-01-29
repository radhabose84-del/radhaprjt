using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class rolemodulemenushema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "RoleModule",
                newName: "RoleModule",
                newSchema: "AppSecurity");

            migrationBuilder.RenameTable(
                name: "RoleMenu",
                newName: "RoleMenu",
                newSchema: "AppSecurity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "RoleModule",
                schema: "AppSecurity",
                newName: "RoleModule");

            migrationBuilder.RenameTable(
                name: "RoleMenu",
                schema: "AppSecurity",
                newName: "RoleMenu");
        }
    }
}
