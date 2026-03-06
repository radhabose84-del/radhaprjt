using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalRequestColumnAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "ApproverBinding",
                schema: "AppData",
                table: "ApprovalRequestLine",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApproverValue",
                schema: "AppData",
                table: "ApprovalRequestLine",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkflowType",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproverBinding",
                schema: "AppData",
                table: "ApprovalRequestLine");

            migrationBuilder.DropColumn(
                name: "ApproverValue",
                schema: "AppData",
                table: "ApprovalRequestLine");

            migrationBuilder.DropColumn(
                name: "WorkflowType",
                schema: "AppData",
                table: "ApprovalRequest");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "int",
                nullable: false,
                defaultValue: 0);

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
    }
}
