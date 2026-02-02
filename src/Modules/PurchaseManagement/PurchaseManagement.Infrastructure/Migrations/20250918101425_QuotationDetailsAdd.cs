using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuotationDetailsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationComparisonDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "QuotationComparisonHeader",
                schema: "Purchase");

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedQuantity",
                schema: "Purchase",
                table: "IndentDetail",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
