using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class pogst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IGSTPercentage",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CGSTPercentage",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SGSTPercentage",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IGSTPercentage",
                schema: "Purchase",
                table: "PurchaseOrderImportDetail");

            migrationBuilder.DropColumn(
                name: "CGSTPercentage",
                schema: "Purchase",
                table: "PurchaseLocalDetail");

            migrationBuilder.DropColumn(
                name: "SGSTPercentage",
                schema: "Purchase",
                table: "PurchaseLocalDetail");
        }
    }
}
