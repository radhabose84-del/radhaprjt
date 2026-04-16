using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalWeightAndDiscountPerUnitToAmendmentDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPerUnit",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWeight",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPerUnit",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail");

            migrationBuilder.DropColumn(
                name: "TotalWeight",
                schema: "Sales",
                table: "SalesOrderAmendmentDetail");
        }
    }
}
