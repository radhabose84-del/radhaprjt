using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Quotation_Entry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_MiscMaster_FreightModeId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_MiscMaster_IncotermsId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_MiscMaster_MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_MiscMaster_MiscMasterId1",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_MiscMaster_MiscMasterId2",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_MiscMaster_PaymentTermsId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropIndex(
                name: "IX_QuotationHeader_MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropIndex(
                name: "IX_QuotationHeader_MiscMasterId1",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropIndex(
                name: "IX_QuotationHeader_MiscMasterId2",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropColumn(
                name: "MiscMasterId1",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropColumn(
                name: "MiscMasterId2",
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

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_Status",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "StatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
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

            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_Status",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId1",
                schema: "Purchase",
                table: "QuotationHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId2",
                schema: "Purchase",
                table: "QuotationHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_MiscMasterId1",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId1");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_MiscMasterId2",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId2");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_MiscMaster_FreightModeId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "FreightModeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_MiscMaster_IncotermsId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "IncotermsId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_MiscMaster_MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_MiscMaster_MiscMasterId1",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId1",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_MiscMaster_MiscMasterId2",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId2",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_MiscMaster_PaymentTermsId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "PaymentTermsId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "StatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
