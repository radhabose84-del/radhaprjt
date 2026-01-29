using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class roleEntitlementModuleFKremove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleEntitlements_Modules_ModuleId",
                schema: "AppSecurity",
                table: "RoleEntitlements");

            migrationBuilder.DropIndex(
                name: "IX_RoleEntitlements_ModuleId",
                schema: "AppSecurity",
                table: "RoleEntitlements");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                schema: "AppSecurity",
                table: "RoleEntitlements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModuleId",
                schema: "AppSecurity",
                table: "RoleEntitlements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RoleEntitlements_ModuleId",
                schema: "AppSecurity",
                table: "RoleEntitlements",
                column: "ModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleEntitlements_Modules_ModuleId",
                schema: "AppSecurity",
                table: "RoleEntitlements",
                column: "ModuleId",
                principalSchema: "AppData",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
