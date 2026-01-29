using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvaltargetMiscremove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalTarget_MiscMaster_MiscMasterId",
                schema: "AppData",
                table: "ApprovalTarget");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalTarget_MiscMasterId",
                schema: "AppData",
                table: "ApprovalTarget");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "AppData",
                table: "ApprovalTarget");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "AppData",
                table: "ApprovalTarget",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalTarget_MiscMasterId",
                schema: "AppData",
                table: "ApprovalTarget",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalTarget_MiscMaster_MiscMasterId",
                schema: "AppData",
                table: "ApprovalTarget",
                column: "MiscMasterId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
