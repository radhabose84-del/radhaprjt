using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPackYearToProductionPackHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionPackHeader_PackNo",
                schema: "Production",
                table: "ProductionPackHeader");

            migrationBuilder.AddColumn<int>(
                name: "PackYear",
                schema: "Production",
                table: "ProductionPackHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_PackNo_PackYear",
                schema: "Production",
                table: "ProductionPackHeader",
                columns: new[] { "PackNo", "PackYear" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionPackHeader_PackNo_PackYear",
                schema: "Production",
                table: "ProductionPackHeader");

            migrationBuilder.DropColumn(
                name: "PackYear",
                schema: "Production",
                table: "ProductionPackHeader");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_PackNo",
                schema: "Production",
                table: "ProductionPackHeader",
                column: "PackNo",
                unique: true);
        }
    }
}
