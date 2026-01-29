using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class companysettingFKadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
 migrationBuilder.CreateIndex(
                name: "IX_CompanySetting_CurrencyId",
                schema: "AppData",
                table: "CompanySetting",
                column: "CurrencyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySetting_FinancialYearId",
                schema: "AppData",
                table: "CompanySetting",
                column: "FinancialYearId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanySetting_LanguageId",
                schema: "AppData",
                table: "CompanySetting",
                column: "LanguageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySetting_Currency_CurrencyId",
                schema: "AppData",
                table: "CompanySetting",
                column: "CurrencyId",
                principalSchema: "AppData",
                principalTable: "Currency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySetting_FinancialYear_FinancialYearId",
                schema: "AppData",
                table: "CompanySetting",
                column: "FinancialYearId",
                principalSchema: "AppData",
                principalTable: "FinancialYear",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySetting_Language_LanguageId",
                schema: "AppData",
                table: "CompanySetting",
                column: "LanguageId",
                principalSchema: "AppData",
                principalTable: "Language",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
migrationBuilder.DropForeignKey(
                name: "FK_CompanySetting_Currency_CurrencyId",
                schema: "AppData",
                table: "CompanySetting");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanySetting_FinancialYear_FinancialYearId",
                schema: "AppData",
                table: "CompanySetting");

            migrationBuilder.DropForeignKey(
                name: "FK_CompanySetting_Language_LanguageId",
                schema: "AppData",
                table: "CompanySetting");

           

            migrationBuilder.DropIndex(
                name: "IX_CompanySetting_CurrencyId",
                schema: "AppData",
                table: "CompanySetting");

            migrationBuilder.DropIndex(
                name: "IX_CompanySetting_FinancialYearId",
                schema: "AppData",
                table: "CompanySetting");

            migrationBuilder.DropIndex(
                name: "IX_CompanySetting_LanguageId",
                schema: "AppData",
                table: "CompanySetting");
        }
    }
}
