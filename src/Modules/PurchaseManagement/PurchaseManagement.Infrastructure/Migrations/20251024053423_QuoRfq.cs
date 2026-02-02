using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuoRfq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_RfqId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "RfqId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuotationHeader_RfqMaster_RfqId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "RfqId",
                principalSchema: "Purchase",
                principalTable: "RfqMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuotationHeader_RfqMaster_RfqId",
                schema: "Purchase",
                table: "QuotationHeader");

            migrationBuilder.DropIndex(
                name: "IX_QuotationHeader_RfqId",
                schema: "Purchase",
                table: "QuotationHeader");
        }
    }
}
