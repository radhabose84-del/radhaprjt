using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransportModeToDeliveryChallanHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransportModeId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_TransportModeId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "TransportModeId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryChallanHeader_MiscMaster_TransportModeId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "TransportModeId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryChallanHeader_MiscMaster_TransportModeId",
                schema: "Sales",
                table: "DeliveryChallanHeader");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryChallanHeader_TransportModeId",
                schema: "Sales",
                table: "DeliveryChallanHeader");

            migrationBuilder.DropColumn(
                name: "TransportModeId",
                schema: "Sales",
                table: "DeliveryChallanHeader");
        }
    }
}
