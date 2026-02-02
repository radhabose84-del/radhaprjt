using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class pricemasterremoveapprove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_FreightMode",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_Incoterms",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_PaymentTerms",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_FreightMode",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "FreightModeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_Incoterms",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "IncotermsId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_PaymentTerms",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "PaymentTermsId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_FreightMode",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_Incoterms",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_PaymentTerms",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_FreightMode",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "FreightModeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_Incoterms",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "IncotermsId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_PaymentTerms",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "PaymentTermsId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
