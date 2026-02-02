using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class importexch_rate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TTExchangeRate",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                type: "decimal(18,5)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TTExchangeRate",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");
        }
    }
}
