using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userdivision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DivisionId",
                schema: "AppSecurity",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UserDivision",
                schema: "AppSecurity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDivision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDivision_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "AppData",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDivision_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "AppSecurity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDivision_DivisionId",
                schema: "AppSecurity",
                table: "UserDivision",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDivision_UserId",
                schema: "AppSecurity",
                table: "UserDivision",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDivision",
                schema: "AppSecurity");

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                schema: "AppSecurity",
                table: "Users",
                type: "int",
                nullable: true);
        }
    }
}
