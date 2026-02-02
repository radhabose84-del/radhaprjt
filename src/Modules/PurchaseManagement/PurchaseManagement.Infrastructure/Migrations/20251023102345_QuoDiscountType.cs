using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuoDiscountType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountTypeId",
                schema: "Purchase",
                table: "QuotationDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuotationDetail_DiscountTypeId",
                schema: "Purchase",
                table: "QuotationDetail",
                column: "DiscountTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationDetail_MiscMaster_DiscountTypeId",
                schema: "Purchase",
                table: "QuotationDetail",
                column: "DiscountTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationDetail_MiscMaster_DiscountTypeId",
                schema: "Purchase",
                table: "QuotationDetail");

            migrationBuilder.DropIndex(
                name: "IX_QuotationDetail_DiscountTypeId",
                schema: "Purchase",
                table: "QuotationDetail");

            migrationBuilder.DropColumn(
                name: "DiscountTypeId",
                schema: "Purchase",
                table: "QuotationDetail");
        }
    }
}
