using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderHeader_EnquiryTypeForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesQuotationHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_EnquiryType",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "EnquiryType");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_SalesQuotationHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesQuotationHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_EnquiryType",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "EnquiryType",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesOrderHeader_SalesQuotationHeader_SalesQuotationHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesQuotationHeaderId",
                principalSchema: "Sales",
                principalTable: "SalesQuotationHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_MiscMaster_EnquiryType",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesOrderHeader_SalesQuotationHeader_SalesQuotationHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_EnquiryType",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesOrderHeader_SalesQuotationHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader");

            migrationBuilder.DropColumn(
                name: "SalesQuotationHeaderId",
                schema: "Sales",
                table: "SalesOrderHeader");
        }
    }
}
