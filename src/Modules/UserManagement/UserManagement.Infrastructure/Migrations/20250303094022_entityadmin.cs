using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class entityadmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                schema: "AppSecurity",
                table: "AdminSecuritySettings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminSecuritySettings_EntityId",
                schema: "AppSecurity",
                table: "AdminSecuritySettings",
                column: "EntityId",
                unique: true,
                filter: "[EntityId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminSecuritySettings_Entity_EntityId",
                schema: "AppSecurity",
                table: "AdminSecuritySettings",
                column: "EntityId",
                principalSchema: "AppData",
                principalTable: "Entity",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminSecuritySettings_Entity_EntityId",
                schema: "AppSecurity",
                table: "AdminSecuritySettings");

            migrationBuilder.DropIndex(
                name: "IX_AdminSecuritySettings_EntityId",
                schema: "AppSecurity",
                table: "AdminSecuritySettings");

            migrationBuilder.DropColumn(
                name: "EntityId",
                schema: "AppSecurity",
                table: "AdminSecuritySettings");
        }
    }
}
