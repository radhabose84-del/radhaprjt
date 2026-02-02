using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class grnputawaytblchangesmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QcAcceptedQuantity",
                schema: "Purchase",
                table: "GrnPutAwayRule",
                newName: "QcAcceptedQtyStockUom");

            migrationBuilder.AddColumn<decimal>(
                name: "ConversionFactor",
                schema: "Purchase",
                table: "GrnPutAwayRule",
                type: "decimal(18,3)",
                nullable: true,
                defaultValue: 0.000m);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseUomId",
                schema: "Purchase",
                table: "GrnPutAwayRule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "QcAcceptedQtyPurchaseUom",
                schema: "Purchase",
                table: "GrnPutAwayRule",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0.000m);

            migrationBuilder.AddColumn<int>(
                name: "StockUomId",
                schema: "Purchase",
                table: "GrnPutAwayRule",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversionFactor",
                schema: "Purchase",
                table: "GrnPutAwayRule");

            migrationBuilder.DropColumn(
                name: "PurchaseUomId",
                schema: "Purchase",
                table: "GrnPutAwayRule");

            migrationBuilder.DropColumn(
                name: "QcAcceptedQtyPurchaseUom",
                schema: "Purchase",
                table: "GrnPutAwayRule");

            migrationBuilder.DropColumn(
                name: "StockUomId",
                schema: "Purchase",
                table: "GrnPutAwayRule");

            migrationBuilder.RenameColumn(
                name: "QcAcceptedQtyStockUom",
                schema: "Purchase",
                table: "GrnPutAwayRule",
                newName: "QcAcceptedQuantity");
        }
    }
}
