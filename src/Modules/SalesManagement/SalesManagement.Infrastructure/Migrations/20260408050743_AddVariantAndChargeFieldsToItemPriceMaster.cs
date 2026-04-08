using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantAndChargeFieldsToItemPriceMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemPriceMaster_ItemId_SalesSegmentId",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalValue",
                schema: "Sales",
                table: "ItemPriceMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CharityValue",
                schema: "Sales",
                table: "ItemPriceMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HandlingCharges",
                schema: "Sales",
                table: "ItemPriceMaster",
                type: "decimal(18,4)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                schema: "Sales",
                table: "ItemPriceMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPriceMaster_ItemId_VariantId_SalesSegmentId",
                schema: "Sales",
                table: "ItemPriceMaster",
                columns: new[] { "ItemId", "VariantId", "SalesSegmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ItemPriceMaster_VariantId",
                schema: "Sales",
                table: "ItemPriceMaster",
                column: "VariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemPriceMaster_ItemId_VariantId_SalesSegmentId",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.DropIndex(
                name: "IX_ItemPriceMaster_VariantId",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.DropColumn(
                name: "AdditionalValue",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.DropColumn(
                name: "CharityValue",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.DropColumn(
                name: "HandlingCharges",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.DropColumn(
                name: "VariantId",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPriceMaster_ItemId_SalesSegmentId",
                schema: "Sales",
                table: "ItemPriceMaster",
                columns: new[] { "ItemId", "SalesSegmentId" });
        }
    }
}
