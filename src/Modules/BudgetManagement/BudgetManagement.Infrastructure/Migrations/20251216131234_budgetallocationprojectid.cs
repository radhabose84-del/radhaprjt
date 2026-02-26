using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS8981

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class budgetallocationprojectid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_CAPEX_Uniq",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.AddColumn<int>(
                name: "WBSId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                schema: "Budget",
                table: "BudgetAllocation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WBSId",
                schema: "Budget",
                table: "BudgetAllocation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_CAPEX_Uniq",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "FinancialYearId", "RequestTypeId", "ProjectId", "WBSId", "FromDate", "ToDate", "RequestById" },
                unique: true,
                filter: "[ProjectId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_CAPEX_Uniq",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "WBSId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "Budget",
                table: "BudgetAllocation");

            migrationBuilder.DropColumn(
                name: "WBSId",
                schema: "Budget",
                table: "BudgetAllocation");

            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_CAPEX_Uniq",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "FinancialYearId", "RequestTypeId", "ProjectId", "FromDate", "ToDate", "RequestById" },
                unique: true,
                filter: "[ProjectId] IS NOT NULL");
        }
    }
}
