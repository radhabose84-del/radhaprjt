using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Importpurchase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportPODetail_ImportPOHeader_PurchaseHeaderId",
                schema: "Purchase",
                table: "ImportPODetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_PurchaseOrderHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPODetail_ImportPOHeader_PurchaseHeaderId",
                schema: "Purchase",
                table: "ImportPODetail",
                column: "PurchaseHeaderId",
                principalSchema: "Purchase",
                principalTable: "ImportPOHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "TTExchangeRateId",
                principalSchema: "Purchase",
                principalTable: "ExchangeRate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_PurchaseOrderHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "PurchaseOrderId",
                principalSchema: "Purchase",
                principalTable: "PurchaseOrderHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportPODetail_ImportPOHeader_PurchaseHeaderId",
                schema: "Purchase",
                table: "ImportPODetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportPOHeader_PurchaseOrderHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "ImportPOHeader");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPODetail_ImportPOHeader_PurchaseHeaderId",
                schema: "Purchase",
                table: "ImportPODetail",
                column: "PurchaseHeaderId",
                principalSchema: "Purchase",
                principalTable: "ImportPOHeader",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_ExchangeRate_TTExchangeRateId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "TTExchangeRateId",
                principalSchema: "Purchase",
                principalTable: "ExchangeRate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImportPOHeader_PurchaseOrderHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "ImportPOHeader",
                column: "PurchaseOrderId",
                principalSchema: "Purchase",
                principalTable: "PurchaseOrderHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
