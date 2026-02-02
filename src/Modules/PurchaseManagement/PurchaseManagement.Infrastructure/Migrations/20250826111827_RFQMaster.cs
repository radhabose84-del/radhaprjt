using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RFQMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RfqStatus",
                schema: "Purchase",
                table: "RfqMaster",
                newName: "RfqStatusId");

            migrationBuilder.RenameColumn(
                name: "InitiationType",
                schema: "Purchase",
                table: "RfqMaster",
                newName: "InitiationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RfqMaster_InitiationTypeId",
                schema: "Purchase",
                table: "RfqMaster",
                column: "InitiationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RfqMaster_RfqStatusId",
                schema: "Purchase",
                table: "RfqMaster",
                column: "RfqStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_RfqMaster_MiscMaster_InitiationTypeId",
                schema: "Purchase",
                table: "RfqMaster",
                column: "InitiationTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RfqMaster_MiscMaster_RfqStatusId",
                schema: "Purchase",
                table: "RfqMaster",
                column: "RfqStatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RfqMaster_MiscMaster_InitiationTypeId",
                schema: "Purchase",
                table: "RfqMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_RfqMaster_MiscMaster_RfqStatusId",
                schema: "Purchase",
                table: "RfqMaster");

            migrationBuilder.DropIndex(
                name: "IX_RfqMaster_InitiationTypeId",
                schema: "Purchase",
                table: "RfqMaster");

            migrationBuilder.DropIndex(
                name: "IX_RfqMaster_RfqStatusId",
                schema: "Purchase",
                table: "RfqMaster");

            migrationBuilder.RenameColumn(
                name: "RfqStatusId",
                schema: "Purchase",
                table: "RfqMaster",
                newName: "RfqStatus");

            migrationBuilder.RenameColumn(
                name: "InitiationTypeId",
                schema: "Purchase",
                table: "RfqMaster",
                newName: "InitiationType");
        }
    }
}
