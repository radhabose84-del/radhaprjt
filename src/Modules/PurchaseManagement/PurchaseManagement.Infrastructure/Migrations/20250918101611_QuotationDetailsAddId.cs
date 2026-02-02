using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuotationDetailsAddId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuotationDetailId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationComparisonDetail_QuotationDetailId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                column: "QuotationDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationComparisonDetail_QuotationDetail_QuotationDetailId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                column: "QuotationDetailId",
                principalSchema: "Purchase",
                principalTable: "QuotationDetail",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationComparisonDetail_QuotationDetail_QuotationDetailId",
                schema: "Purchase",
                table: "QuotationComparisonDetail");

            migrationBuilder.DropIndex(
                name: "IX_QuotationComparisonDetail_QuotationDetailId",
                schema: "Purchase",
                table: "QuotationComparisonDetail");

            migrationBuilder.DropColumn(
                name: "QuotationDetailId",
                schema: "Purchase",
                table: "QuotationComparisonDetail");
        }
    }
}
