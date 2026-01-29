using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class companysettingFKcompany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CompanySetting_CompanyId",
                schema: "AppData",
                table: "CompanySetting",
                column: "CompanyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySetting_Company_CompanyId",
                schema: "AppData",
                table: "CompanySetting",
                column: "CompanyId",
                principalSchema: "AppData",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanySetting_Company_CompanyId",
                schema: "AppData",
                table: "CompanySetting");

            migrationBuilder.DropIndex(
                name: "IX_CompanySetting_CompanyId",
                schema: "AppData",
                table: "CompanySetting");
        }
    }
}
