using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class companyrelationshiponetoone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyContact_CompanyId",
                schema: "AppData",
                table: "CompanyContact");

            migrationBuilder.DropIndex(
                name: "IX_CompanyAddress_CompanyId",
                schema: "AppData",
                table: "CompanyAddress");


            migrationBuilder.CreateIndex(
                name: "IX_CompanyContact_CompanyId",
                schema: "AppData",
                table: "CompanyContact",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAddress_CompanyId",
                schema: "AppData",
                table: "CompanyAddress",
                column: "CompanyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CompanyContact_CompanyId",
                schema: "AppData",
                table: "CompanyContact");

            migrationBuilder.DropIndex(
                name: "IX_CompanyAddress_CompanyId",
                schema: "AppData",
                table: "CompanyAddress");


            migrationBuilder.CreateIndex(
                name: "IX_CompanyContact_CompanyId",
                schema: "AppData",
                table: "CompanyContact",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAddress_CompanyId",
                schema: "AppData",
                table: "CompanyAddress",
                column: "CompanyId");
        }
    }
}
