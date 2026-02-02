using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateEntryRecetypechanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GateEntryDetail_MiscMaster_PoTypeId",
                schema: "Purchase",
                table: "GateEntryDetail");

            migrationBuilder.RenameColumn(
                name: "PoTypeId",
                schema: "Purchase",
                table: "GateEntryDetail",
                newName: "PoCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_GateEntryDetail_PoTypeId",
                schema: "Purchase",
                table: "GateEntryDetail",
                newName: "IX_GateEntryDetail_PoCategoryId");

            migrationBuilder.AddColumn<int>(
                name: "ReceivingTypeId",
                schema: "Purchase",
                table: "GateEntryHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GateEntryHeader_ReceivingTypeId",
                schema: "Purchase",
                table: "GateEntryHeader",
                column: "ReceivingTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_GateEntryDetail_MiscMaster_PoCategoryId",
                schema: "Purchase",
                table: "GateEntryDetail",
                column: "PoCategoryId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GateEntryHeader_MiscMaster_ReceivingTypeId",
                schema: "Purchase",
                table: "GateEntryHeader",
                column: "ReceivingTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GateEntryDetail_MiscMaster_PoCategoryId",
                schema: "Purchase",
                table: "GateEntryDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_GateEntryHeader_MiscMaster_ReceivingTypeId",
                schema: "Purchase",
                table: "GateEntryHeader");

            migrationBuilder.DropIndex(
                name: "IX_GateEntryHeader_ReceivingTypeId",
                schema: "Purchase",
                table: "GateEntryHeader");

            migrationBuilder.DropColumn(
                name: "ReceivingTypeId",
                schema: "Purchase",
                table: "GateEntryHeader");

            migrationBuilder.RenameColumn(
                name: "PoCategoryId",
                schema: "Purchase",
                table: "GateEntryDetail",
                newName: "PoTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_GateEntryDetail_PoCategoryId",
                schema: "Purchase",
                table: "GateEntryDetail",
                newName: "IX_GateEntryDetail_PoTypeId");

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
    }
}
