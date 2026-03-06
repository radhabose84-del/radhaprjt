using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fkchanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRule_MiscMaster_WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_WorkFlowTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRule_WorkflowType_WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRule",
                column: "WorkflowTypeId",
                principalSchema: "AppData",
                principalTable: "WorkflowType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalStepDetail_WorkflowType_WorkFlowTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "WorkFlowTypeId",
                principalSchema: "AppData",
                principalTable: "WorkflowType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRule_WorkflowType_WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalStepDetail_WorkflowType_WorkFlowTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRule_MiscMaster_WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRule",
                column: "WorkflowTypeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_WorkFlowTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "WorkFlowTypeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
