using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_Unwanted_MiscMasterId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceEntrySheets_MiscMaster_MiscMasterId",
                schema: "Purchase",
                table: "ServiceEntrySheets");

            migrationBuilder.DropIndex(
                name: "IX_ServiceEntrySheets_MiscMasterId",
                schema: "Purchase",
                table: "ServiceEntrySheets");

            migrationBuilder.DropColumn(
                name: "CompletionStatus",
                schema: "Purchase",
                table: "ServiceEntrySheets");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "Purchase",
                table: "ServiceEntrySheets");

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Purchase",
                table: "ServiceEntrySheets");

            migrationBuilder.AddColumn<string>(
                name: "CompletionStatus",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEntrySheets_MiscMasterId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceEntrySheets_MiscMaster_MiscMasterId",
                schema: "Purchase",
                table: "ServiceEntrySheets",
                column: "MiscMasterId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
