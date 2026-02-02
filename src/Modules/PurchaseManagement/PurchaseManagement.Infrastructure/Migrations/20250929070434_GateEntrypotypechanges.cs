using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateEntrypotypechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GateEntryHeader_MiscMaster_PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader");

            migrationBuilder.DropIndex(
                name: "IX_GateEntryHeader_PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader");

            migrationBuilder.DropColumn(
                name: "PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader");

            migrationBuilder.AddColumn<int>(
                name: "PoTypeId",
                schema: "Purchase",
                table: "GateEntryDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GateEntryDetail_PoTypeId",
                schema: "Purchase",
                table: "GateEntryDetail",
                column: "PoTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GateEntryDetail_MiscMaster_PoTypeId",
                schema: "Purchase",
                table: "GateEntryDetail",
                column: "PoTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GateEntryDetail_MiscMaster_PoTypeId",
                schema: "Purchase",
                table: "GateEntryDetail");

            migrationBuilder.DropIndex(
                name: "IX_GateEntryDetail_PoTypeId",
                schema: "Purchase",
                table: "GateEntryDetail");

            migrationBuilder.DropColumn(
                name: "PoTypeId",
                schema: "Purchase",
                table: "GateEntryDetail");

            migrationBuilder.AddColumn<int>(
                name: "PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GateEntryHeader_PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader",
                column: "PoTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GateEntryHeader_MiscMaster_PoTypeId",
                schema: "Purchase",
                table: "GateEntryHeader",
                column: "PoTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
