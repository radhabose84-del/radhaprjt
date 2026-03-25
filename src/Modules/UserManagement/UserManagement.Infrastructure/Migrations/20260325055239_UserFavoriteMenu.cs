using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserFavoriteMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserFavoriteMenu",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoriteMenu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavoriteMenu_Menus_MenuId",
                        column: x => x.MenuId,
                        principalSchema: "AppData",
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteMenu_MenuId",
                schema: "AppData",
                table: "UserFavoriteMenu",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteMenu_UserId",
                schema: "AppData",
                table: "UserFavoriteMenu",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoriteMenu_UserId_MenuId",
                schema: "AppData",
                table: "UserFavoriteMenu",
                columns: new[] { "UserId", "MenuId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserFavoriteMenu",
                schema: "AppData");
        }
    }
}
