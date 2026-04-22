using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceDetailBagWeightNetWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                schema: "Sales",
                table: "InvoiceDetail",
                newName: "NetWeight");

            migrationBuilder.AlterColumn<decimal>(
                name: "NoOfBags",
                schema: "Sales",
                table: "InvoiceDetail",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "BagWeight",
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
                name: "BagWeight",
                schema: "Sales",
                table: "InvoiceDetail");

            migrationBuilder.RenameColumn(
                name: "NetWeight",
                schema: "Sales",
                table: "InvoiceDetail",
                newName: "Quantity");

            migrationBuilder.AlterColumn<int>(
                name: "NoOfBags",
                schema: "Sales",
                table: "InvoiceDetail",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,6)",
                oldPrecision: 18,
                oldScale: 6);
        }
    }
}
