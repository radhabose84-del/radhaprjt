using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class rolemodulemenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleEntitlements_Menus_MenuId",
                schema: "AppSecurity",
                table: "RoleEntitlements");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "AppData",
                table: "Menus",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "AppData",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                schema: "AppData",
                table: "Menus",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedIP",
                schema: "AppData",
                table: "Menus",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "AppData",
                table: "Menus",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MenuIcon",
                schema: "AppData",
                table: "Menus",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MenuUrl",
                schema: "AppData",
                table: "Menus",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "AppData",
                table: "Menus",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "AppData",
                table: "Menus",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Menus",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Menus",
                type: "varchar(255)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentId",
                schema: "AppData",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "AppData",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "RoleMenu",
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
                    table.PrimaryKey("PK_RoleMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleMenu_Menus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "AppData",
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleMenu_UserRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AppSecurity",
                        principalTable: "UserRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleModule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleModule_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "AppData",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleModule_UserRole_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "AppSecurity",
                        principalTable: "UserRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenu_MenuId",
                table: "RoleMenu",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleMenu_RoleId",
                table: "RoleMenu",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModule_ModuleId",
                table: "RoleModule",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleModule_RoleId",
                table: "RoleModule",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleEntitlements_Menus_MenuId",
                schema: "AppSecurity",
                table: "RoleEntitlements",
                column: "MenuId",
                principalSchema: "AppData",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleEntitlements_Menus_MenuId",
                schema: "AppSecurity",
                table: "RoleEntitlements");

            migrationBuilder.DropTable(
                name: "RoleMenu");

            migrationBuilder.DropTable(
                name: "RoleModule");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "CreatedIP",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "MenuIcon",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "MenuUrl",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "AppData",
                table: "Menus");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleEntitlements_Menus_MenuId",
                schema: "AppSecurity",
                table: "RoleEntitlements",
                column: "MenuId",
                principalSchema: "AppData",
                principalTable: "Menus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
