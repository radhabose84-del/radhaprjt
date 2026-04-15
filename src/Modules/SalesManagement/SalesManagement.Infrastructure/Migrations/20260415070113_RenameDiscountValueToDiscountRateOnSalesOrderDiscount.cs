using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameDiscountValueToDiscountRateOnSalesOrderDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DiscountValue",
                schema: "Sales",
                table: "SalesOrderDiscount",
                newName: "TotalDiscountValue");

            migrationBuilder.AddColumn<decimal>(
                name: "MdDiscountPercentage",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MdDiscountValue",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDiscountValue",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                schema: "Sales",
                table: "SalesOrderDiscount",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MdDiscountPercentage",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "MdDiscountValue",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "TotalDiscountValue",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "DiscountRate",
                schema: "Sales",
                table: "SalesOrderDiscount");

            migrationBuilder.RenameColumn(
                name: "TotalDiscountValue",
                schema: "Sales",
                table: "SalesOrderDiscount",
                newName: "DiscountValue");
        }
    }
}
