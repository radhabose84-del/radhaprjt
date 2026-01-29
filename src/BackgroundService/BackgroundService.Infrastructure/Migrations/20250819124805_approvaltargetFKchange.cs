using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvaltargetFKchange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "FK_ApprovalTarget_ApprovalStepDetail_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalTarget",
                column: "ApprovalStepDetailId",
                principalSchema: "AppData",
                principalTable: "ApprovalStepDetail",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalTarget_MiscMaster_MiscMasterId",
                schema: "AppData",
                table: "ApprovalTarget",
                column: "MiscMasterId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalTarget_ApprovalStepDetail_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalTarget");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalTarget_MiscMaster_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalTarget",
                column: "ApprovalStepId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
