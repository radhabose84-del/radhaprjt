using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculationTypeToVendorEvaluationCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalculationType",
                schema: "Purchase",
                table: "VendorEvaluationCriteria",
                type: "varchar(30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalculationType",
                schema: "Purchase",
                table: "VendorEvaluationCriteria");
        }
    }
}
