using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalRequestRuleFKchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRule_MiscMaster_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.RenameColumn(
                name: "ApprovalStepId",
                schema: "AppData",
                table: "ApprovalRule",
                newName: "ApprovalStepDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalRule_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalRule",
                newName: "IX_ApprovalRule_ApprovalStepDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRule_ApprovalStepDetail_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalRule",
                column: "ApprovalStepDetailId",
                principalSchema: "AppData",
                principalTable: "ApprovalStepDetail",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRule_ApprovalStepDetail_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.RenameColumn(
                name: "ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalRule",
                newName: "ApprovalStepId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalRule_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalRule",
                newName: "IX_ApprovalRule_ApprovalStepId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRule_MiscMaster_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalRule",
                column: "ApprovalStepId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
