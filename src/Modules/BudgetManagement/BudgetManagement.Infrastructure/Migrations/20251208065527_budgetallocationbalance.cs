using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class budgetallocationbalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "LastMonthUtilized",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "RemainingBalance",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.AlterColumn<int>(
                name: "BudgetGroupId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingBalance",
                schema: "Budget",
                table: "BudgetAllocation",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetRequest_BudgetGroupId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "BudgetGroupId");

            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "DepartmentId", "CostCenterId", "FinYearId", "RequestMonthId", "RequestTypeId", "BudgetGroupId" },
                unique: true,
                filter: "[RequestMonthId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetRequest_BudgetGroup_BudgetGroupId",
                schema: "Budget",
                table: "BudgetRequest",
                column: "BudgetGroupId",
                principalSchema: "Budget",
                principalTable: "BudgetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetRequest_BudgetGroup_BudgetGroupId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropIndex(
                name: "IX_BudgetRequest_BudgetGroupId",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropColumn(
                name: "RemainingBalance",
                schema: "Budget",
                table: "BudgetAllocation");

            migrationBuilder.AlterColumn<int>(
                name: "BudgetGroupId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "LastMonthUtilized",
                schema: "Budget",
                table: "BudgetRequest",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingBalance",
                schema: "Budget",
                table: "BudgetRequest",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "DepartmentId", "CostCenterId", "FinYearId", "RequestMonthId", "RequestTypeId", "BudgetGroupId" },
                unique: true,
                filter: "[RequestMonthId] IS NOT NULL AND [BudgetGroupId] IS NOT NULL");
        }
    }
}
