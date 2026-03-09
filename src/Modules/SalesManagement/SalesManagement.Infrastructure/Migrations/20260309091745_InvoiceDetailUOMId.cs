using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceDetailUOMId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UOM",
                schema: "Sales",
                table: "InvoiceDetail");

            migrationBuilder.AddColumn<int>(
                name: "UOMId",
                schema: "Sales",
                table: "InvoiceDetail",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UOMId",
                schema: "Sales",
                table: "InvoiceDetail");

            migrationBuilder.AddColumn<string>(
                name: "UOM",
                schema: "Sales",
                table: "InvoiceDetail",
                type: "varchar(20)",
                nullable: true);
        }
    }
}
