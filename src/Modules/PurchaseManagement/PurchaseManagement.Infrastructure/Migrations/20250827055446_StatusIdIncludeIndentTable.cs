using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StatusIdIncludeIndentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Purchase",
                table: "IndentHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "Purchase",
                table: "IndentDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_IndentHeader_StatusId",
                schema: "Purchase",
                table: "IndentHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_IndentDetail_StatusId",
                schema: "Purchase",
                table: "IndentDetail",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndentDetail_MiscMaster_StatusId",
                schema: "Purchase",
                table: "IndentDetail",
                column: "StatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_IndentHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "IndentHeader",
                column: "StatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndentDetail_MiscMaster_StatusId",
                schema: "Purchase",
                table: "IndentDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_IndentHeader_MiscMaster_StatusId",
                schema: "Purchase",
                table: "IndentHeader");

            migrationBuilder.DropIndex(
                name: "IX_IndentHeader_StatusId",
                schema: "Purchase",
                table: "IndentHeader");

            migrationBuilder.DropIndex(
                name: "IX_IndentDetail_StatusId",
                schema: "Purchase",
                table: "IndentDetail");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Purchase",
                table: "IndentHeader");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "Purchase",
                table: "IndentDetail");
        }
    }
}
