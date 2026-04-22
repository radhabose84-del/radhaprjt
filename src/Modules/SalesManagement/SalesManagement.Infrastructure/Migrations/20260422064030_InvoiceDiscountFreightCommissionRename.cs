using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceDiscountFreightCommissionRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Freight",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "FreightValue");

            migrationBuilder.RenameColumn(
                name: "Discount",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "DiscountValue");

            migrationBuilder.RenameColumn(
                name: "Discount",
                schema: "Sales",
                table: "InvoiceDetail",
                newName: "FreightValue");

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionValue",
                schema: "Sales",
                table: "InvoiceHeader",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionValue",
                schema: "Sales",
                table: "InvoiceDetail",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                schema: "Sales",
                table: "InvoiceDetail",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CommissionValue",
                schema: "Sales",
                table: "InvoiceHeader");

            migrationBuilder.DropColumn(
                name: "CommissionValue",
                schema: "Sales",
                table: "InvoiceDetail");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                schema: "Sales",
                table: "InvoiceDetail");

            migrationBuilder.RenameColumn(
                name: "FreightValue",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "Freight");

            migrationBuilder.RenameColumn(
                name: "DiscountValue",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "Discount");

            migrationBuilder.RenameColumn(
                name: "FreightValue",
                schema: "Sales",
                table: "InvoiceDetail",
                newName: "Discount");
        }
    }
}
