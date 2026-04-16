using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHandlingAndCharityToSalesOrderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Charity",
                schema: "Sales",
                table: "SalesOrderDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Handling",
                schema: "Sales",
                table: "SalesOrderDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Charity",
                schema: "Sales",
                table: "SalesOrderDetail");

            migrationBuilder.DropColumn(
                name: "Handling",
                schema: "Sales",
                table: "SalesOrderDetail");
        }
    }
}
