using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class singleDepartmentperIndent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IndentDepartmentMapping",
                schema: "Purchase");

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                schema: "Purchase",
                table: "IndentHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "Purchase",
                table: "IndentHeader");

            migrationBuilder.CreateTable(
                name: "IndentDepartmentMapping",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IndentHeaderId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndentDepartmentMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IndentDepartmentMapping_IndentHeader_IndentHeaderId",
                        column: x => x.IndentHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "IndentHeader",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndentDepartmentMapping_IndentHeaderId",
                schema: "Purchase",
                table: "IndentDepartmentMapping",
                column: "IndentHeaderId");
        }
    }
}
