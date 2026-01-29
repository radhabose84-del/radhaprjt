using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackgroundService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class approvalflowColumnChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RuleGroupId",
                schema: "AppData",
                table: "RuleSkipApproverMapping",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                schema: "AppData",
                table: "ApprovalRule",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RuleGroupId",
                schema: "AppData",
                table: "ApprovalRule",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                schema: "AppData",
                table: "ApprovalRequest",
                type: "Varchar(50)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RuleGroupId",
                schema: "AppData",
                table: "RuleSkipApproverMapping");

            migrationBuilder.DropColumn(
                name: "DataType",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "RuleGroupId",
                schema: "AppData",
                table: "ApprovalRule");

            migrationBuilder.DropColumn(
                name: "Action",
                schema: "AppData",
                table: "ApprovalRequest");
        }
    }
}
