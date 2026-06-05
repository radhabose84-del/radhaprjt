using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OCRRateUnitToInventoryUom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OCREntry_MiscMaster_RateUnitId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropIndex(
                name: "IX_OCREntry_RateUnitId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.RenameColumn(
                name: "RateUnitId",
                schema: "Purchase",
                table: "OCREntry",
                newName: "UomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UomId",
                schema: "Purchase",
                table: "OCREntry",
                newName: "RateUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_RateUnitId",
                schema: "Purchase",
                table: "OCREntry",
                column: "RateUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_OCREntry_MiscMaster_RateUnitId",
                schema: "Purchase",
                table: "OCREntry",
                column: "RateUnitId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
