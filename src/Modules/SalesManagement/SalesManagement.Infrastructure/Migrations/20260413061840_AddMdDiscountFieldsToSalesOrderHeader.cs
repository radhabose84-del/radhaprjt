using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMdDiscountFieldsToSalesOrderHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMdDiscountEnabled",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MdApprovalDocument",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "varchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MdDiscountRate",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMdDiscountEnabled",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "MdApprovalDocument",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "MdDiscountRate",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
