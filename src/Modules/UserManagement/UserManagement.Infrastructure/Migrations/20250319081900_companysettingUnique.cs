using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class companysettingUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CompanySetting_CompanyId_CurrencyId_FinancialYearId_LanguageId",
                schema: "AppData",
                table: "CompanySetting",
                columns: new[] { "CompanyId", "CurrencyId", "FinancialYearId", "LanguageId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanySetting_CompanyId_CurrencyId_FinancialYearId_LanguageId",
                schema: "AppData",
                table: "CompanySetting");
        }
    }
}
