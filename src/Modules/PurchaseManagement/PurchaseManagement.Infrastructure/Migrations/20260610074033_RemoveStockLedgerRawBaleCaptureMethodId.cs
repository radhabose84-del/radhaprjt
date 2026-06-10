using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStockLedgerRawBaleCaptureMethodId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaleCaptureMethodId",
                schema: "Purchase",
                table: "StockLedgerRaw");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BaleCaptureMethodId",
                schema: "Purchase",
                table: "StockLedgerRaw",
                type: "int",
                nullable: true);
        }
    }
}
