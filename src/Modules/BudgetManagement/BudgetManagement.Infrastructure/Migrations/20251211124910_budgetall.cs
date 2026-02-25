using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS8981

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class budgetall : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "BudgetSbuGroupId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "DepartmentId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "FinYearId", "RequestMonthId", "RequestTypeId", "BudgetGroupId" },
                unique: true,
                filter: "[RequestMonthId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.AddColumn<int>(
                name: "BudgetSbuGroupId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DepartmentId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "DepartmentId", "CostCenterId", "FinYearId", "RequestMonthId", "RequestTypeId", "BudgetGroupId" },
                unique: true,
                filter: "[RequestMonthId] IS NOT NULL");
        }
    }
}
