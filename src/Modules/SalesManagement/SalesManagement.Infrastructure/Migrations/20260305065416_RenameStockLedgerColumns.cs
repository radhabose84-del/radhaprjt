using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameStockLedgerColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockLedger_DocType_DocNo_PackNo",
                schema: "Sales",
                table: "StockLedger");

            migrationBuilder.RenameColumn(
                name: "DocSno",
                schema: "Sales",
                table: "StockLedger",
                newName: "ProductionPackHeaderId");

            migrationBuilder.RenameColumn(
                name: "DocNo",
                schema: "Sales",
                table: "StockLedger",
                newName: "DetailDocNo");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedger_DocType_ProductionPackHeaderId_PackNo",
                schema: "Sales",
                table: "StockLedger",
                columns: new[] { "DocType", "ProductionPackHeaderId", "PackNo" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockLedger_DocType_ProductionPackHeaderId_PackNo",
                schema: "Sales",
                table: "StockLedger");

            migrationBuilder.RenameColumn(
                name: "ProductionPackHeaderId",
                schema: "Sales",
                table: "StockLedger",
                newName: "DocSno");

            migrationBuilder.RenameColumn(
                name: "DetailDocNo",
                schema: "Sales",
                table: "StockLedger",
                newName: "DocNo");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedger_DocType_DocNo_PackNo",
                schema: "Sales",
                table: "StockLedger",
                columns: new[] { "DocType", "DocNo", "PackNo" },
                unique: true);
        }
    }
}
