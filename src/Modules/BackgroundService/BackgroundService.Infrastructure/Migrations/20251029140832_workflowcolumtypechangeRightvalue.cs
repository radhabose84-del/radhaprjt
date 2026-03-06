using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class workflowcolumtypechangeRightvalue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovalRuleCondition_MiscMaster_RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropIndex(
                name: "IX_ApprovalRuleCondition_RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.DropColumn(
                name: "RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.AddColumn<string>(
                name: "RightValue",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                type: "Varchar(250)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RightValue",
                schema: "AppData",
                table: "ApprovalRuleCondition");

            migrationBuilder.AddColumn<int>(
                name: "RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRuleCondition_RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "RightValueId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovalRuleCondition_MiscMaster_RightValueId",
                schema: "AppData",
                table: "ApprovalRuleCondition",
                column: "RightValueId",
                principalSchema: "AppData",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
