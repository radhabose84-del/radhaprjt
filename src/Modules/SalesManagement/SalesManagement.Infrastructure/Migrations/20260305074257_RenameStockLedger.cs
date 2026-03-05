using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameStockLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductionPackHeaderId",
                schema: "Sales",
                table: "StockLedger",
                newName: "DocNo");

            migrationBuilder.RenameIndex(
                name: "IX_StockLedger_DocType_ProductionPackHeaderId_PackNo",
                schema: "Sales",
                table: "StockLedger",
                newName: "IX_StockLedger_DocType_DocNo_PackNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DocNo",
                schema: "Sales",
                table: "StockLedger",
                newName: "ProductionPackHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_StockLedger_DocType_DocNo_PackNo",
                schema: "Sales",
                table: "StockLedger",
                newName: "IX_StockLedger_DocType_ProductionPackHeaderId_PackNo");
        }
    }
}
