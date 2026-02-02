using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class pobudgetfinyear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BudgetMonthId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinancialYearId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BudgetMonthId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");

            migrationBuilder.DropColumn(
                name: "FinancialYearId",
                schema: "Purchase",
                table: "PurchaseOrderHeader");
        }
    }
}
