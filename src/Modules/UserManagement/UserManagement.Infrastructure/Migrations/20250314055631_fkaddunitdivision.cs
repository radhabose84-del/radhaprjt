using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fkaddunitdivision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Unit_CompanyId",
                schema: "AppData",
                table: "Unit",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Unit_DivisionId",
                schema: "AppData",
                table: "Unit",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Division_CompanyId",
                schema: "AppData",
                table: "Division",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Division_Company_CompanyId",
                schema: "AppData",
                table: "Division",
                column: "CompanyId",
                principalSchema: "AppData",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Unit_Company_CompanyId",
                schema: "AppData",
                table: "Unit",
                column: "CompanyId",
                principalSchema: "AppData",
                principalTable: "Company",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Unit_Division_DivisionId",
                schema: "AppData",
                table: "Unit",
                column: "DivisionId",
                principalSchema: "AppData",
                principalTable: "Division",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Division_Company_CompanyId",
                schema: "AppData",
                table: "Division");

            migrationBuilder.DropForeignKey(
                name: "FK_Unit_Company_CompanyId",
                schema: "AppData",
                table: "Unit");

            migrationBuilder.DropForeignKey(
                name: "FK_Unit_Division_DivisionId",
                schema: "AppData",
                table: "Unit");

            migrationBuilder.DropIndex(
                name: "IX_Unit_CompanyId",
                schema: "AppData",
                table: "Unit");

            migrationBuilder.DropIndex(
                name: "IX_Unit_DivisionId",
                schema: "AppData",
                table: "Unit");

            migrationBuilder.DropIndex(
                name: "IX_Division_CompanyId",
                schema: "AppData",
                table: "Division");
        }
    }
}
