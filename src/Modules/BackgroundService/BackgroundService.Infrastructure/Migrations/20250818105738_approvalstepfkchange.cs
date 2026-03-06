using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalstepfkchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRule_ApprovalStepDetail_ApprovalDetailId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalTarget_ApprovalStepDetail_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalTarget");

            migrationBuilder.RenameColumn(
                name: "ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalTarget",
                newName: "ApprovalStepId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalTarget_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalTarget",
                newName: "IX_ApprovalTarget_ApprovalStepId");

            migrationBuilder.RenameColumn(
                name: "ApprovalDetailId",
                schema: "AppData",
                table: "ApprovalRule",
                newName: "ApprovalStepId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalRule_ApprovalDetailId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalTarget_MiscMaster_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalTarget",
                column: "ApprovalStepId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRule_MiscMaster_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalTarget_MiscMaster_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalTarget");

            migrationBuilder.RenameColumn(
                name: "ApprovalStepId",
                schema: "AppData",
                table: "ApprovalTarget",
                newName: "ApprovalStepDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalTarget_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalTarget",
                newName: "IX_ApprovalTarget_ApprovalStepDetailId");

            migrationBuilder.RenameColumn(
                name: "ApprovalStepId",
                schema: "AppData",
                table: "ApprovalRule",
                newName: "ApprovalDetailId");

            migrationBuilder.RenameIndex(
                name: "IX_ApprovalRule_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalRule",
                newName: "IX_ApprovalRule_ApprovalDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRule_ApprovalStepDetail_ApprovalDetailId",
                schema: "AppData",
                table: "ApprovalRule",
                column: "ApprovalDetailId",
                principalSchema: "AppData",
                principalTable: "ApprovalStepDetail",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalTarget_ApprovalStepDetail_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalTarget",
                column: "ApprovalStepDetailId",
                principalSchema: "AppData",
                principalTable: "ApprovalStepDetail",
                principalColumn: "Id");
        }
    }
}
