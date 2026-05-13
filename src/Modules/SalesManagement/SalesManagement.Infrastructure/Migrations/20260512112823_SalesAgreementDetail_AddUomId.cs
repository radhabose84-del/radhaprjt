using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesAgreementDetail_AddUomId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UomId",
                schema: "Sales",
                table: "SalesAgreementDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgreementDetail_UomId",
                schema: "Sales",
                table: "SalesAgreementDetail",
                column: "UomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalesAgreementDetail_UomId",
                schema: "Sales",
                table: "SalesAgreementDetail");

            migrationBuilder.DropColumn(
                name: "UomId",
                schema: "Sales",
                table: "SalesAgreementDetail");
        }
    }
}
