using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Quotation_Validate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_Status",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropIndex(
                name: "IX_QuotationHeader_StatusId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceCharge",
                schema: "Purchase",
                table: "QuotationHeader",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValidityDays",
                schema: "Purchase",
                table: "QuotationDetail",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_MiscMaster_MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_MiscMaster_MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropIndex(
                name: "IX_QuotationHeader_MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropColumn(
                name: "InsuranceCharge",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropColumn(
                name: "ValidityDays",
                schema: "Purchase",
                table: "QuotationDetail");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Purchase",
                table: "QuotationHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_StatusId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "StatusId");

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
    }
}
