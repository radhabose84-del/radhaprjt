using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class rolechild : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleMenu",
                schema: "AppSecurity");

            migrationBuilder.CreateTable(
                name: "RoleChild",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleChild", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleChild_Menus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "AppData",
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleChild_UserRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AppSecurity",
                        principalTable: "UserRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleMenuPrivilege",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false),
                    CanAdd = table.Column<bool>(type: "bit", nullable: false),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CanExport = table.Column<bool>(type: "bit", nullable: false),
                    CanApprove = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenuPrivilege", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenuPrivilege_Menus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "AppData",
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleMenuPrivilege_UserRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AppSecurity",
                        principalTable: "UserRole",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleChild_MenuId",
                table: "RoleChild",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleChild_RoleId",
                table: "RoleChild",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuPrivilege_MenuId",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenuPrivilege_RoleId",
                schema: "AppSecurity",
                table: "RoleMenuPrivilege",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleChild");

            migrationBuilder.DropTable(
                name: "RoleMenuPrivilege",
                schema: "AppSecurity");

            migrationBuilder.CreateTable(
                name: "RoleMenu",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    RoleModuleId = table.Column<int>(type: "int", nullable: false),
                    CanAdd = table.Column<bool>(type: "bit", nullable: false),
                    CanApprove = table.Column<bool>(type: "bit", nullable: false),
                    CanDelete = table.Column<bool>(type: "bit", nullable: false),
                    CanExport = table.Column<bool>(type: "bit", nullable: false),
                    CanUpdate = table.Column<bool>(type: "bit", nullable: false),
                    CanView = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenu_Menus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "AppData",
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleMenu_RoleModule_RoleModuleId",
                        column: x => x.RoleModuleId,
                        principalSchema: "AppSecurity",
                        principalTable: "RoleModule",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenu_MenuId",
                schema: "AppSecurity",
                table: "RoleMenu",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenu_RoleModuleId",
                schema: "AppSecurity",
                table: "RoleMenu",
                column: "RoleModuleId");
        }
    }
}
