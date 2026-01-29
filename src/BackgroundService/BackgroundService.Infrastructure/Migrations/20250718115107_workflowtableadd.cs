using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class workflowtableadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalRule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConditionKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WorkflowTypeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalRule_MiscMaster_WorkflowTypeId",
                        column: x => x.WorkflowTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalStepDetail",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkFlowTypeId = table.Column<int>(type: "int", nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    TargetTypeId = table.Column<int>(type: "int", nullable: false),
                    ApprovalStepId = table.Column<int>(type: "int", nullable: false),
                    ApprovalTypeId = table.Column<int>(type: "int", nullable: false),
                    SLAHours = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OnSLAAction = table.Column<string>(type: "varchar(50)", nullable: true),
                    ApprovalStepId1 = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalStepDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalStepDetail_MiscMaster_ApprovalStepId",
                        column: x => x.ApprovalStepId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalStepDetail_MiscMaster_ApprovalStepId1",
                        column: x => x.ApprovalStepId1,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovalStepDetail_MiscMaster_WorkFlowTypeId",
                        column: x => x.WorkFlowTypeId,
                        principalSchema: "AppData",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WorkflowType",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    ModuleTypeName = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalStepUnitMapping",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalStepDetailId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalStepUnitMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalStepUnitMapping_ApprovalStepDetail_ApprovalStepDetailId",
                        column: x => x.ApprovalStepDetailId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalStepDetail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RuleSkipApproverMapping",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleId = table.Column<int>(type: "int", nullable: false),
                    ApprovalDetailId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleSkipApproverMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleSkipApproverMapping_ApprovalStepDetail_ApprovalDetailId",
                        column: x => x.ApprovalDetailId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalStepDetail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRule_WorkflowTypeId",
                table: "ApprovalRule",
                column: "WorkflowTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepDetail_ApprovalStepId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "ApprovalStepId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepDetail_ApprovalStepId1",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "ApprovalStepId1");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepDetail_WorkFlowTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "WorkFlowTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepUnitMapping_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalStepUnitMapping",
                column: "ApprovalStepDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleSkipApproverMapping_ApprovalDetailId",
                schema: "AppData",
                table: "RuleSkipApproverMapping",
                column: "ApprovalDetailId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalRule");

            migrationBuilder.DropTable(
                name: "ApprovalStepUnitMapping",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "RuleSkipApproverMapping",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "WorkflowType",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "ApprovalStepDetail",
                schema: "AppData");
        }
    }
}
