using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesQuotationHeaderStatusId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Sales",
                table: "SalesQuotationHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Sales",
                table: "ItemPriceMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHeader_StatusId",
                schema: "Sales",
                table: "SalesQuotationHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPriceMaster_StatusId",
                schema: "Sales",
                table: "ItemPriceMaster",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemPriceMaster_MiscMaster_StatusId",
                schema: "Sales",
                table: "ItemPriceMaster",
                column: "StatusId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesQuotationHeader_MiscMaster_StatusId",
                schema: "Sales",
                table: "SalesQuotationHeader",
                column: "StatusId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemPriceMaster_MiscMaster_StatusId",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesQuotationHeader_MiscMaster_StatusId",
                schema: "Sales",
                table: "SalesQuotationHeader");

            migrationBuilder.DropIndex(
                name: "IX_SalesQuotationHeader_StatusId",
                schema: "Sales",
                table: "SalesQuotationHeader");

            migrationBuilder.DropIndex(
                name: "IX_ItemPriceMaster_StatusId",
                schema: "Sales",
                table: "ItemPriceMaster");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Sales",
                table: "SalesQuotationHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Sales",
                table: "ItemPriceMaster");
        }
    }
}
