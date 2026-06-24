using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OCRRemoveBrokerDirect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OCREntry_MiscMaster_BrokerDirectId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropIndex(
                name: "IX_OCREntry_BrokerDirectId",
                schema: "Purchase",
                table: "OCREntry");

            migrationBuilder.DropColumn(
                name: "BrokerDirectId",
                schema: "Purchase",
                table: "OCREntry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BrokerDirectId",
                schema: "Purchase",
                table: "OCREntry",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_BrokerDirectId",
                schema: "Purchase",
                table: "OCREntry",
                column: "BrokerDirectId");

            migrationBuilder.AddForeignKey(
                name: "FK_OCREntry_MiscMaster_BrokerDirectId",
                schema: "Purchase",
                table: "OCREntry",
                column: "BrokerDirectId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
