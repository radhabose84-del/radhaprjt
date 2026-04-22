using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceTypeIdToInvoiceHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FreightValue",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "TotalFreight");

            migrationBuilder.RenameColumn(
                name: "DiscountValue",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "TotalDiscount");

            migrationBuilder.RenameColumn(
                name: "CommissionValue",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "TotalCommission");

            migrationBuilder.AddColumn<int>(
                name: "InvoiceTypeId",
                schema: "Sales",
                table: "InvoiceHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_InvoiceTypeId",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "InvoiceTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceHeader_InvoiceTypeId",
                schema: "Sales",
                table: "InvoiceHeader");

            migrationBuilder.DropColumn(
                name: "InvoiceTypeId",
                schema: "Sales",
                table: "InvoiceHeader");

            migrationBuilder.RenameColumn(
                name: "TotalFreight",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "FreightValue");

            migrationBuilder.RenameColumn(
                name: "TotalDiscount",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "DiscountValue");

            migrationBuilder.RenameColumn(
                name: "TotalCommission",
                schema: "Sales",
                table: "InvoiceHeader",
                newName: "CommissionValue");
        }
    }
}
