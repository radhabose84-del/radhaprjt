using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesQuotationDetail_VariantUomDiscountType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountTypeId",
                schema: "Sales",
                table: "SalesQuotationDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UOMId",
                schema: "Sales",
                table: "SalesQuotationDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                schema: "Sales",
                table: "SalesQuotationDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDetail_DiscountTypeId",
                schema: "Sales",
                table: "SalesQuotationDetail",
                column: "DiscountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDetail_UOMId",
                schema: "Sales",
                table: "SalesQuotationDetail",
                column: "UOMId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDetail_VariantId",
                schema: "Sales",
                table: "SalesQuotationDetail",
                column: "VariantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesQuotationDetail_DiscountTypeId",
                schema: "Sales",
                table: "SalesQuotationDetail");

            migrationBuilder.DropIndex(
                name: "IX_SalesQuotationDetail_UOMId",
                schema: "Sales",
                table: "SalesQuotationDetail");

            migrationBuilder.DropIndex(
                name: "IX_SalesQuotationDetail_VariantId",
                schema: "Sales",
                table: "SalesQuotationDetail");

            migrationBuilder.DropColumn(
                name: "DiscountTypeId",
                schema: "Sales",
                table: "SalesQuotationDetail");

            migrationBuilder.DropColumn(
                name: "UOMId",
                schema: "Sales",
                table: "SalesQuotationDetail");

            migrationBuilder.DropColumn(
                name: "VariantId",
                schema: "Sales",
                table: "SalesQuotationDetail");
        }
    }
}
