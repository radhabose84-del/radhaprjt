using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Quotationdetailnamechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationComparisonDetail_QuotationComparisonHeader_QuotationConfirmedHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail");

            migrationBuilder.RenameColumn(
                name: "QuotationConfirmedHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                newName: "QuotationComparisonHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationComparisonDetail_QuotationConfirmedHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                newName: "IX_QuotationComparisonDetail_QuotationComparisonHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationComparisonDetail_QuotationComparisonHeader_QuotationComparisonHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                column: "QuotationComparisonHeaderId",
                principalSchema: "Purchase",
                principalTable: "QuotationComparisonHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationComparisonDetail_QuotationComparisonHeader_QuotationComparisonHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail");

            migrationBuilder.RenameColumn(
                name: "QuotationComparisonHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                newName: "QuotationConfirmedHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_QuotationComparisonDetail_QuotationComparisonHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                newName: "IX_QuotationComparisonDetail_QuotationConfirmedHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationComparisonDetail_QuotationComparisonHeader_QuotationConfirmedHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                column: "QuotationConfirmedHeaderId",
                principalSchema: "Purchase",
                principalTable: "QuotationComparisonHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
