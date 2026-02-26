using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS8981

namespace BudgetManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class budgetgroupid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ToDate",
                schema: "Budget",
                table: "BudgetRequest",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FromDate",
                schema: "Budget",
                table: "BudgetRequest",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "Budget",
                table: "BudgetRequest",
                type: "varchar(20)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "Budget",
                table: "BudgetRequest",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<int>(
                name: "BudgetGroupId",
                schema: "Budget",
                table: "BudgetRequest",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BudgetGroupId",
                schema: "Budget",
                table: "BudgetAllocation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_CAPEX_Uniq",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "FinancialYearId", "RequestTypeId", "ProjectId", "FromDate", "ToDate", "RequestById" },
                unique: true,
                filter: "[ProjectId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_OPEX_Uniq",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "FinancialYearId", "RequestTypeId", "BudgetGroupId", "FromDate", "ToDate", "RequestById" },
                unique: true,
                filter: "[ProjectId] IS NULL AND [BudgetGroupId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BudgetRequest_Project_vs_BudgetGroup",
                schema: "Budget",
                table: "BudgetRequest",
                sql: "([ProjectId] IS NOT NULL AND [BudgetGroupId] IS NULL) OR ([ProjectId] IS NULL AND [BudgetGroupId] IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_CAPEX_Uniq",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropIndex(
                name: "UX_BudgetRequest_OPEX_Uniq",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BudgetRequest_Project_vs_BudgetGroup",
                schema: "Budget",
                table: "BudgetRequest");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ToDate",
                schema: "Budget",
                table: "BudgetRequest",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "FromDate",
                schema: "Budget",
                table: "BudgetRequest",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "Budget",
                table: "BudgetRequest",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "Budget",
                table: "BudgetRequest",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

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

            migrationBuilder.AlterColumn<int>(
                name: "BudgetGroupId",
                schema: "Budget",
                table: "BudgetAllocation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "UX_BudgetRequest_Uniq_Month",
                schema: "Budget",
                table: "BudgetRequest",
                columns: new[] { "UnitId", "FinancialYearId", "RequestTypeId", "BudgetGroupId", "FromDate", "ToDate", "RequestById" },
                unique: true,
                filter: "[RequestById] IS NOT NULL");
        }
    }
}
