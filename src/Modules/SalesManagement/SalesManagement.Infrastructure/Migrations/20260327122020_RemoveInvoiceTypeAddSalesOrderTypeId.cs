using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInvoiceTypeAddSalesOrderTypeId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceHeader_MiscMaster_InvoiceType",
                schema: "Sales",
                table: "InvoiceHeader");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceHeader_InvoiceType",
                schema: "Sales",
                table: "InvoiceHeader");

            migrationBuilder.DropColumn(
                name: "InvoiceType",
                schema: "Sales",
                table: "InvoiceHeader");

            migrationBuilder.AddColumn<int>(
                name: "SalesOrderTypeId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesOrderTypeId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.AddColumn<int>(
                name: "InvoiceType",
                schema: "Sales",
                table: "InvoiceHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_InvoiceType",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "InvoiceType");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceHeader_MiscMaster_InvoiceType",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "InvoiceType",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
