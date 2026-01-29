using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalLineFKStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequestLine_StatusId",
                schema: "AppData",
                table: "ApprovalRequestLine",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRequestLine_MiscMaster_StatusId",
                schema: "AppData",
                table: "ApprovalRequestLine",
                column: "StatusId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRequestLine_MiscMaster_StatusId",
                schema: "AppData",
                table: "ApprovalRequestLine");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRequestLine_StatusId",
                schema: "AppData",
                table: "ApprovalRequestLine");
        }
    }
}
