using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class shippingmodeid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShippingModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderImportHeader_ShippingModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "ShippingModeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_ShippingModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader",
                column: "ShippingModeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderImportHeader_MiscMaster_ShippingModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderImportHeader_ShippingModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");

            migrationBuilder.DropColumn(
                name: "ShippingModeId",
                schema: "Purchase",
                table: "PurchaseOrderImportHeader");
        }
    }
}
