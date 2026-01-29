using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalcolumnremoveRulegroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RuleGroupId",
                schema: "AppData",
                table: "RuleSkipApproverMapping");

            migrationBuilder.DropColumn(
                name: "RuleGroupId",
                schema: "AppData",
                table: "ApprovalRule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RuleGroupId",
                schema: "AppData",
                table: "RuleSkipApproverMapping",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RuleGroupId",
                schema: "AppData",
                table: "ApprovalRule",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");
        }
    }
}
