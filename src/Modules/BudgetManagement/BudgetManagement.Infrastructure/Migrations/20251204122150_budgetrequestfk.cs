using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class budgetrequestfk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "DepartmentId", "CostCenterId", "FinYearId", "RequestMonthId", "BudgetGroupId" },
                unique: true,
                filter: "[RequestMonthId] IS NOT NULL AND [BudgetGroupId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest");
        }
    }
}
