using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class quotationremovestatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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
                name: "MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader",
                type: "int",
                nullable: true);

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
    }
}
