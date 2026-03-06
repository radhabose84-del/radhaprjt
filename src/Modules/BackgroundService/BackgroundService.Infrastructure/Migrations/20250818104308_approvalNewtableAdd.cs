using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalNewtableAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropTable(
                name: "ApprovalStepDepartmentMapping",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "RuleSkipApproverMapping",
                schema: "AppData");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalStepDetail_ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "OnSLAAction",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "SLAHours",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "TargetTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "ConditionKey",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "DataType",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "Operator",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "Value",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "StopOnFirstMatch",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalDetailId",
                schema: "AppData",
                table: "ApprovalRule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveFrom",
                schema: "AppData",
                table: "ApprovalRule",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "EffectiveTo",
                schema: "AppData",
                table: "ApprovalRule",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "AppData",
                table: "ApprovalRule",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ApprovalDataField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JsonPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValueType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_ApprovalDataField", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalTarget",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalStepDetailId = table.Column<int>(type: "int", nullable: false),
                    Binding = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", nullable: false),
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
                    table.PrimaryKey("PK_ApprovalTarget", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalTarget_ApprovalStepDetail_ApprovalStepDetailId",
                        column: x => x.ApprovalStepDetailId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalStepDetail",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RuleTargetOverride",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleId = table.Column<int>(type: "int", nullable: false),
                    Binding = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(200)", nullable: false),
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
                    table.PrimaryKey("PK_RuleTargetOverride", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RuleTargetOverride_ApprovalRule_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalRule",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApprovalRuleCondition",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleId = table.Column<int>(type: "int", nullable: false),
                    GroupKey = table.Column<int>(type: "int", nullable: false),
                    FieldId = table.Column<int>(type: "int", nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    RightType = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    RightValue = table.Column<string>(type: "nvarchar(400)", nullable: false),
                    Aggregate = table.Column<string>(type: "nvarchar(10)", nullable: true),
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
                    table.PrimaryKey("PK_ApprovalRuleCondition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalRuleCondition_ApprovalDataField_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ApprovalDataField",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApprovalRuleCondition_ApprovalRule_RuleId",
                        column: x => x.RuleId,
                        principalSchema: "AppData",
                        principalTable: "ApprovalRule",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepDetail_MiscMasterId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRule_ApprovalDetailId",
                schema: "AppData",
                table: "ApprovalRule",
                column: "ApprovalDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRuleCondition_FieldId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRuleCondition_RuleId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalTarget_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalTarget",
                column: "ApprovalStepDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleTargetOverride_RuleId",
                schema: "AppData",
                table: "RuleTargetOverride",
                column: "RuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRule_ApprovalStepDetail_ApprovalDetailId",
                schema: "AppData",
                table: "ApprovalRule",
                column: "ApprovalDetailId",
                principalSchema: "AppData",
                principalTable: "ApprovalStepDetail",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_MiscMasterId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "MiscMasterId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRule_ApprovalStepDetail_ApprovalDetailId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_MiscMasterId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropTable(
                name: "ApprovalRuleCondition",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "ApprovalTarget",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "RuleTargetOverride",
                schema: "AppData");

            migrationBuilder.DropTable(
                name: "ApprovalDataField");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalStepDetail_MiscMasterId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRule_ApprovalDetailId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "StopOnFirstMatch",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "ApprovalDetailId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.AddColumn<int>(
                name: "ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OnSLAAction",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SLAHours",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConditionKey",
                schema: "AppData",
                table: "ApprovalRule",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                schema: "AppData",
                table: "ApprovalRule",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                schema: "AppData",
                table: "ApprovalRule",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                schema: "AppData",
                table: "ApprovalRule",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ApprovalStepDepartmentMapping",
                schema: "AppData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalStepDetailId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalStepDepartmentMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalStepDepartmentMapping_ApprovalStepDetail_ApprovalStepDetailId",
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
                    ApprovalDetailId = table.Column<int>(type: "int", nullable: false),
                    RuleId = table.Column<int>(type: "int", nullable: false)
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
                name: "IX_ApprovalStepDetail_ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "ApprovalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepDepartmentMapping_ApprovalStepDetailId",
                schema: "AppData",
                table: "ApprovalStepDepartmentMapping",
                column: "ApprovalStepDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleSkipApproverMapping_ApprovalDetailId",
                schema: "AppData",
                table: "RuleSkipApproverMapping",
                column: "ApprovalDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_ApprovalTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "ApprovalTypeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
