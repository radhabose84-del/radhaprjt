using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderHeader_AddSalesAgreementHeaderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesAgreementHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_SalesAgreementHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesAgreementHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_SalesAgreementHeader_SalesAgreementHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesAgreementHeaderId",
                principalSchema: "Sales",
                principalTable: "SalesAgreementHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_SalesAgreementHeader_SalesAgreementHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_SalesAgreementHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "SalesAgreementHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
