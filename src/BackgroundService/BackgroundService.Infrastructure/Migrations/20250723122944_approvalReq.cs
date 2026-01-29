using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalReq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalRequest",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowTypeId = table.Column<int>(type: "int", nullable: false),
                    ModuleTransactionId = table.Column<int>(type: "int", nullable: false),
                    ApprovalStepDetailId = table.Column<int>(type: "int", nullable: false),
                    ApprovalRuleId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    RequestedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalRequest_ApprovalRule_ApprovalRuleId",
                        column: x => x.ApprovalRuleId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalRule",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalRequest_ApprovalStepDetail_ApprovalStepDetailId",
                        column: x => x.ApprovalStepDetailId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalStepDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalRequest_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalRequest_WorkflowType_WorkflowTypeId",
                        column: x => x.WorkflowTypeId,
                        principalSchema: "AppData",
                        principalTable: "WorkflowType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequest_ApprovalRuleId",
                schema: "AppData",
                table: "ApprovalRequest",
                column: "ApprovalRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequest_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalRequest",
                column: "ApprovalStepDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequest_StatusId",
                schema: "AppData",
                table: "ApprovalRequest",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequest_WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRequest",
                column: "WorkflowTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalRequest",
                schema: "AppData");
        }
    }
}
