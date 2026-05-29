using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBlanketPaymentDeliveryTerms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryTerms",
                schema: "Purchase",
                table: "BlanketHeader");

            migrationBuilder.DropColumn(
                name: "PaymentTerms",
                schema: "Purchase",
                table: "BlanketHeader");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryTerms",
                schema: "Purchase",
                table: "BlanketHeader",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTerms",
                schema: "Purchase",
                table: "BlanketHeader",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
