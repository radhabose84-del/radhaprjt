using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class quoteValidityDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedQuantity",
                schema: "Purchase",
                table: "IndentDetail");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValidityDays",
                schema: "Purchase",
                table: "QuotationDetail",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryDays",
                schema: "Purchase",
                table: "QuotationDetail",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryDays",
                schema: "Purchase",
                table: "QuotationDetail");

            migrationBuilder.AlterColumn<decimal>(
                name: "ValidityDays",
                schema: "Purchase",
                table: "QuotationDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedQuantity",
                schema: "Purchase",
                table: "IndentDetail",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
