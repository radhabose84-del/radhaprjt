using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GrnQc_MoveFromHeaderToDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrnHeader_MiscMaster_QcStatusId",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropIndex(
                name: "IX_GrnHeader_QcStatusId",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "IsQcApproved",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "QcApprovedIp",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "QcDate",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "QcPersonName",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "QcRemarks",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.DropColumn(
                name: "QcStatusId",
                schema: "Purchase",
                table: "GrnHeader");

            migrationBuilder.AddColumn<bool>(
                name: "IsQcApproved",
                schema: "Purchase",
                table: "GrnDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QcApprovedIp",
                schema: "Purchase",
                table: "GrnDetail",
                type: "varchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "QcDate",
                schema: "Purchase",
                table: "GrnDetail",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QcPersonName",
                schema: "Purchase",
                table: "GrnDetail",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QcRemarks",
                schema: "Purchase",
                table: "GrnDetail",
                type: "varchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QcStatusId",
                schema: "Purchase",
                table: "GrnDetail",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsQcApproved",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "QcApprovedIp",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "QcDate",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "QcPersonName",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "QcRemarks",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.DropColumn(
                name: "QcStatusId",
                schema: "Purchase",
                table: "GrnDetail");

            migrationBuilder.AddColumn<bool>(
                name: "IsQcApproved",
                schema: "Purchase",
                table: "GrnHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "QcApprovedIp",
                schema: "Purchase",
                table: "GrnHeader",
                type: "varchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "QcDate",
                schema: "Purchase",
                table: "GrnHeader",
                type: "DatetimeOffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QcPersonName",
                schema: "Purchase",
                table: "GrnHeader",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QcRemarks",
                schema: "Purchase",
                table: "GrnHeader",
                type: "varchar(250)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QcStatusId",
                schema: "Purchase",
                table: "GrnHeader",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GrnHeader_QcStatusId",
                schema: "Purchase",
                table: "GrnHeader",
                column: "QcStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_GrnHeader_MiscMaster_QcStatusId",
                schema: "Purchase",
                table: "GrnHeader",
                column: "QcStatusId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
