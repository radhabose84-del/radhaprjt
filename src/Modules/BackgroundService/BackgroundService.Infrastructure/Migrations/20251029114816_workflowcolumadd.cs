using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class workflowcolumadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Binding",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "Value",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "Operator",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropColumn(
                name: "RightType",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropColumn(
                name: "RightValue",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropColumn(
                name: "Action",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "Scope",
                schema: "AppData",
                table: "ApprovalDataField");

            migrationBuilder.DropColumn(
                name: "ValueType",
                schema: "AppData",
                table: "ApprovalDataField");

            migrationBuilder.RenameColumn(
                name: "ModuleTypeName",
                schema: "AppData",
                table: "WorkflowType",
                newName: "MenuId");

            migrationBuilder.RenameColumn(
                name: "GroupKey",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                newName: "RightValueId");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                schema: "AppData",
                table: "ApprovalRule",
                newName: "ActionId");

            migrationBuilder.AddColumn<int>(
                name: "TargetTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetValueId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OperatorId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RightTypeId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRule",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ScopeId",
                schema: "AppData",
                table: "ApprovalDataField",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ValueTypeId",
                schema: "AppData",
                table: "ApprovalDataField",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalStepDetail_TargetTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "TargetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRuleCondition_OperatorId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRuleCondition_RightTypeId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "RightTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRuleCondition_RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "RightValueId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRule_ActionId",
                schema: "AppData",
                table: "ApprovalRule",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDataField_ScopeId",
                schema: "AppData",
                table: "ApprovalDataField",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDataField_ValueTypeId",
                schema: "AppData",
                table: "ApprovalDataField",
                column: "ValueTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalDataField_MiscMaster_ScopeId",
                schema: "AppData",
                table: "ApprovalDataField",
                column: "ScopeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalDataField_MiscMaster_ValueTypeId",
                schema: "AppData",
                table: "ApprovalDataField",
                column: "ValueTypeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRule_MiscMaster_ActionId",
                schema: "AppData",
                table: "ApprovalRule",
                column: "ActionId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRuleCondition_MiscMaster_OperatorId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "OperatorId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRuleCondition_MiscMaster_RightTypeId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "RightTypeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRuleCondition_MiscMaster_RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "RightValueId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_TargetTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail",
                column: "TargetTypeId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalDataField_MiscMaster_ScopeId",
                schema: "AppData",
                table: "ApprovalDataField");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalDataField_MiscMaster_ValueTypeId",
                schema: "AppData",
                table: "ApprovalDataField");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRule_MiscMaster_ActionId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRuleCondition_MiscMaster_OperatorId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRuleCondition_MiscMaster_RightTypeId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRuleCondition_MiscMaster_RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalStepDetail_MiscMaster_TargetTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalStepDetail_TargetTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRuleCondition_OperatorId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRuleCondition_RightTypeId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRuleCondition_RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRule_ActionId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalDataField_ScopeId",
                schema: "AppData",
                table: "ApprovalDataField");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalDataField_ValueTypeId",
                schema: "AppData",
                table: "ApprovalDataField");

            migrationBuilder.DropColumn(
                name: "TargetTypeId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "TargetValueId",
                schema: "AppData",
                table: "ApprovalStepDetail");

            migrationBuilder.DropColumn(
                name: "OperatorId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropColumn(
                name: "RightTypeId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropColumn(
                name: "ScopeId",
                schema: "AppData",
                table: "ApprovalDataField");

            migrationBuilder.DropColumn(
                name: "ValueTypeId",
                schema: "AppData",
                table: "ApprovalDataField");

            migrationBuilder.RenameColumn(
                name: "MenuId",
                schema: "AppData",
                table: "WorkflowType",
                newName: "ModuleTypeName");

            migrationBuilder.RenameColumn(
                name: "RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                newName: "GroupKey");

            migrationBuilder.RenameColumn(
                name: "ActionId",
                schema: "AppData",
                table: "ApprovalRule",
                newName: "UnitId");

            migrationBuilder.AddColumn<string>(
                name: "Binding",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                schema: "AppData",
                table: "ApprovalStepDetail",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Operator",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RightType",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RightValue",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                type: "nvarchar(400)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "WorkflowTypeId",
                schema: "AppData",
                table: "ApprovalRule",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Action",
                schema: "AppData",
                table: "ApprovalRule",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                schema: "AppData",
                table: "ApprovalDataField",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ValueType",
                schema: "AppData",
                table: "ApprovalDataField",
                type: "nvarchar(10)",
                nullable: false,
                defaultValue: "");
        }
    }
}
