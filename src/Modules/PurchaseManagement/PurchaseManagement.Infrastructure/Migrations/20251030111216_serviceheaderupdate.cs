using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class serviceheaderupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderServiceSchedule_PurchaseOrderServiceHeader_ServicePoHeaderId1",
                schema: "Purchase",
                table: "PurchaseOrderServiceSchedule");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderServiceSchedule_ServicePoHeaderId1",
                schema: "Purchase",
                table: "PurchaseOrderServiceSchedule");

            migrationBuilder.DropColumn(
                name: "ServicePoHeaderId1",
                schema: "Purchase",
                table: "PurchaseOrderServiceSchedule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServicePoHeaderId1",
                schema: "Purchase",
                table: "PurchaseOrderServiceSchedule",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderServiceSchedule_ServicePoHeaderId1",
                schema: "Purchase",
                table: "PurchaseOrderServiceSchedule",
                column: "ServicePoHeaderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderServiceSchedule_PurchaseOrderServiceHeader_ServicePoHeaderId1",
                schema: "Purchase",
                table: "PurchaseOrderServiceSchedule",
                column: "ServicePoHeaderId1",
                principalSchema: "Purchase",
                principalTable: "PurchaseOrderServiceHeader",
                principalColumn: "Id");
        }
    }
}
