using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemPriceMaster_RemovePaymentTermsAndExMillRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = 'IX_SalesItemPriceMaster_ItemId_SalesSegmentId_PaymentTermsId'
                      AND object_id = OBJECT_ID('Sales.ItemPriceMaster')
                )
                DROP INDEX [IX_SalesItemPriceMaster_ItemId_SalesSegmentId_PaymentTermsId]
                    ON [Sales].[ItemPriceMaster];");

            migrationBuilder.DropColumn(
                name: "ExMillRate",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.DropColumn(
                name: "PaymentTermsId",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPriceMaster_ItemId_SalesSegmentId",
                schema: "Sales",
                table: "ItemPriceMaster",
                columns: new[] { "ItemId", "SalesSegmentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemPriceMaster_ItemId_SalesSegmentId",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.AddColumn<decimal>(
                name: "ExMillRate",
                schema: "Sales",
                table: "ItemPriceMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTermsId",
                schema: "Sales",
                table: "ItemPriceMaster",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPriceMaster_ItemId_SalesSegmentId_PaymentTermsId",
                schema: "Sales",
                table: "ItemPriceMaster",
                columns: new[] { "ItemId", "SalesSegmentId", "PaymentTermsId" });
        }
    }
}
