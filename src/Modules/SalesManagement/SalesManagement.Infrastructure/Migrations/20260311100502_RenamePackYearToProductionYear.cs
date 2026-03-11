using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenamePackYearToProductionYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PackYear",
                schema: "Production",
                table: "ProductionPackHeader",
                newName: "ProductionYear");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionPackHeader_PackNo_PackYear",
                schema: "Production",
                table: "ProductionPackHeader",
                newName: "IX_ProductionPackHeader_PackNo_ProductionYear");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductionYear",
                schema: "Production",
                table: "ProductionPackHeader",
                newName: "PackYear");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionPackHeader_PackNo_ProductionYear",
                schema: "Production",
                table: "ProductionPackHeader",
                newName: "IX_ProductionPackHeader_PackNo_PackYear");
        }
    }
}
