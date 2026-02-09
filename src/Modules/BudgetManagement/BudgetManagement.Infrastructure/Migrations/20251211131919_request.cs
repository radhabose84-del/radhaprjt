using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class request : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FinYearId",
                schema: "Budget",
                table: "BudgetRequest",
                newName: "FinancialYearId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FinancialYearId",
                schema: "Budget",
                table: "BudgetRequest",
                newName: "FinYearId");
        }
    }
}
