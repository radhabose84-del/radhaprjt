using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class JournalMasterRefinements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_LedgerBalance",
                schema: "Finance",
                table: "LedgerBalance");

            migrationBuilder.DropColumn(
                name: "CostCentreKey",
                schema: "Finance",
                table: "LedgerBalance");

            migrationBuilder.CreateIndex(
                name: "UX_LedgerBalance",
                schema: "Finance",
                table: "LedgerBalance",
                columns: new[] { "CompanyId", "GlAccountId", "AccountingPeriodId", "CostCentreId" },
                unique: true,
                filter: "[CostCentreId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_LedgerBalance",
                schema: "Finance",
                table: "LedgerBalance");

            migrationBuilder.AddColumn<int>(
                name: "CostCentreKey",
                schema: "Finance",
                table: "LedgerBalance",
                type: "int",
                nullable: false,
                computedColumnSql: "ISNULL([CostCentreId], 0)",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "UX_LedgerBalance",
                schema: "Finance",
                table: "LedgerBalance",
                columns: new[] { "CompanyId", "GlAccountId", "AccountingPeriodId", "CostCentreKey" },
                unique: true);
        }
    }
}
