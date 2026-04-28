using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesQuotationnumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuotationNo",
                schema: "Sales",
                table: "SalesQuotationHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHeader_QuotationNo",
                schema: "Sales",
                table: "SalesQuotationHeader",
                column: "QuotationNo",
                unique: true,
                filter: "[QuotationNo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesQuotationHeader_QuotationNo",
                schema: "Sales",
                table: "SalesQuotationHeader");

            migrationBuilder.DropColumn(
                name: "QuotationNo",
                schema: "Sales",
                table: "SalesQuotationHeader");
        }
    }
}
