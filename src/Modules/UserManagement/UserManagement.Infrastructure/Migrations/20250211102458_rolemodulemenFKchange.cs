using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class rolemodulemenFKchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenu_UserRole_RoleId",
                schema: "AppSecurity",
                table: "RoleMenu");

            migrationBuilder.RenameColumn(
                name: "RoleId",
                schema: "AppSecurity",
                table: "RoleMenu",
                newName: "RoleModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleMenu_RoleId",
                schema: "AppSecurity",
                table: "RoleMenu",
                newName: "IX_RoleMenu_RoleModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenu_RoleModule_RoleModuleId",
                schema: "AppSecurity",
                table: "RoleMenu",
                column: "RoleModuleId",
                principalSchema: "AppSecurity",
                principalTable: "RoleModule",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleMenu_RoleModule_RoleModuleId",
                schema: "AppSecurity",
                table: "RoleMenu");

            migrationBuilder.RenameColumn(
                name: "RoleModuleId",
                schema: "AppSecurity",
                table: "RoleMenu",
                newName: "RoleId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleMenu_RoleModuleId",
                schema: "AppSecurity",
                table: "RoleMenu",
                newName: "IX_RoleMenu_RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleMenu_UserRole_RoleId",
                schema: "AppSecurity",
                table: "RoleMenu",
                column: "RoleId",
                principalSchema: "AppSecurity",
                principalTable: "UserRole",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
