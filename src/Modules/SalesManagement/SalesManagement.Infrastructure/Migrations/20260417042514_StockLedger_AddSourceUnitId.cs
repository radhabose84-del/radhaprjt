using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class StockLedger_AddSourceUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SourceUnitId",
                schema: "Sales",
                table: "StockLedger",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockLedger_ItemId_UnitId_SourceUnitId",
                schema: "Sales",
                table: "StockLedger",
                columns: new[] { "ItemId", "UnitId", "SourceUnitId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockLedger_ItemId_UnitId_SourceUnitId",
                schema: "Sales",
                table: "StockLedger");

            migrationBuilder.DropColumn(
                name: "SourceUnitId",
                schema: "Sales",
                table: "StockLedger");
        }
    }
}
