using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class roleparent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoleParent",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleParent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleParent_Menus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "AppData",
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleParent_UserRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AppSecurity",
                        principalTable: "UserRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleParent_MenuId",
                schema: "AppSecurity",
                table: "RoleParent",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleParent_RoleId",
                schema: "AppSecurity",
                table: "RoleParent",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleParent",
                schema: "AppSecurity");
        }
    }
}
