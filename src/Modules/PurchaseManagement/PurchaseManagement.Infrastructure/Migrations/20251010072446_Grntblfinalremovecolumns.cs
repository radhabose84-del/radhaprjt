using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Grntblfinalremovecolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPutAwayRuleApproved",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "PutAwayRuleApprovedByName",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "PutAwayRuleApprovedDate",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "PutAwayRuleApprovedIp",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "BinId",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "RackId",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                schema: "Purchase",
                table: "GrnDetail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPutAwayRuleApproved",
                schema: "Purchase",
                table: "GrnHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PutAwayRuleApprovedByName",
                schema: "Purchase",
                table: "GrnHeader",
                type: "nvarchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PutAwayRuleApprovedDate",
                schema: "Purchase",
                table: "GrnHeader",
                type: "DatetimeOffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PutAwayRuleApprovedIp",
                schema: "Purchase",
                table: "GrnHeader",
                type: "varchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BinId",
                schema: "Purchase",
                table: "GrnDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RackId",
                schema: "Purchase",
                table: "GrnDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                schema: "Purchase",
                table: "GrnDetail",
                type: "int",
                nullable: true);
        }
    }
}
