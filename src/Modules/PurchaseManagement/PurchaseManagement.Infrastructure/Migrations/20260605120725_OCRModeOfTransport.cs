using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OCRModeOfTransport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModeOfTransportId",
                schema: "Purchase",
                table: "OCREntry",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_ModeOfTransportId",
                schema: "Purchase",
                table: "OCREntry",
                column: "ModeOfTransportId");

            migrationBuilder.AddForeignKey(
                name: "FK_OCREntry_MiscMaster_ModeOfTransportId",
                schema: "Purchase",
                table: "OCREntry",
                column: "ModeOfTransportId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OCREntry_MiscMaster_ModeOfTransportId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropIndex(
                name: "IX_OCREntry_ModeOfTransportId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "ModeOfTransportId",
                schema: "Purchase",
                table: "OCREntry");
        }
    }
}
