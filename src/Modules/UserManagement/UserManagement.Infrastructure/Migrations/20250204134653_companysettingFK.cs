using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class companysettingFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
           

            migrationBuilder.RenameColumn(
                name: "Language",
                schema: "AppData",
                table: "CompanySetting",
                newName: "LanguageId");

            migrationBuilder.RenameColumn(
                name: "FinancialYear",
                schema: "AppData",
                table: "CompanySetting",
                newName: "FinancialYearId");

            migrationBuilder.RenameColumn(
                name: "Currency",
                schema: "AppData",
                table: "CompanySetting",
                newName: "CurrencyId");

           


           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            


            migrationBuilder.RenameColumn(
                name: "LanguageId",
                schema: "AppData",
                table: "CompanySetting",
                newName: "Language");

            migrationBuilder.RenameColumn(
                name: "FinancialYearId",
                schema: "AppData",
                table: "CompanySetting",
                newName: "FinancialYear");

            migrationBuilder.RenameColumn(
                name: "CurrencyId",
                schema: "AppData",
                table: "CompanySetting",
                newName: "Currency");

            
        }
    }
}
