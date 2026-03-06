using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalsteptablecolumnRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_ApprovalStepId1",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalStepDetail_ApprovalStepId1",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "ApprovalStepId1",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepDetail_ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "ApprovalTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "ApprovalTypeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalStepDetail_ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStepId1",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepDetail_ApprovalStepId1",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "ApprovalStepId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_ApprovalStepId1",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "ApprovalStepId1",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
