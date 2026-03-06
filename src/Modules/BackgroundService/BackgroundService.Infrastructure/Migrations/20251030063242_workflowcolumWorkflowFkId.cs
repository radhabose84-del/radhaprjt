using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class workflowcolumWorkflowFkId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequest_WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRequest",
                column: "WorkflowTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRequest_WorkflowType_WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRequest",
                column: "WorkflowTypeId",
                principalSchema: "AppData",
                principalTable: "WorkflowType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRequest_WorkflowType_WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRequest");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRequest_WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRequest");

            migrationBuilder.DropColumn(
                name: "WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRequest");
        }
    }
}
