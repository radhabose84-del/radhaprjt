using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductionStockLedgerDocTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductionStockLedger_DocTypeId",
                schema: "Production",
                table: "ProductionStockLedger");

            migrationBuilder.DropColumn(
                name: "DocTypeId",
                schema: "Production",
                table: "ProductionStockLedger");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocTypeId",
                schema: "Production",
                table: "ProductionStockLedger",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStockLedger_DocTypeId",
                schema: "Production",
                table: "ProductionStockLedger",
                column: "DocTypeId");
        }
    }
}
