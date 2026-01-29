using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserUnit",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserUnit_Unit_UnitId",
                        column: x => x.UnitId,
                        principalSchema: "AppData",
                        principalTable: "Unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserUnit_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AppSecurity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserUnit_UnitId",
                schema: "AppSecurity",
                table: "UserUnit",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserUnit_UserId",
                schema: "AppSecurity",
                table: "UserUnit",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserUnit",
                schema: "AppSecurity");
        }
    }
}
