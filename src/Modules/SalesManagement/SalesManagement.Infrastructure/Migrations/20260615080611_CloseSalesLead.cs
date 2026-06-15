using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CloseSalesLead : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ClosureDate",
                schema: "Sales",
                table: "SalesLead",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosureReasonId",
                schema: "Sales",
                table: "SalesLead",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosureRemarks",
                schema: "Sales",
                table: "SalesLead",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClosureTypeId",
                schema: "Sales",
                table: "SalesLead",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConvertWonLeadToId",
                schema: "Sales",
                table: "SalesLead",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_ClosureReasonId",
                schema: "Sales",
                table: "SalesLead",
                column: "ClosureReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_ClosureTypeId",
                schema: "Sales",
                table: "SalesLead",
                column: "ClosureTypeId",
                filter: "[ClosureTypeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesLead_ConvertWonLeadToId",
                schema: "Sales",
                table: "SalesLead",
                column: "ConvertWonLeadToId");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesLead_MiscMaster_ClosureReasonId",
                schema: "Sales",
                table: "SalesLead",
                column: "ClosureReasonId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesLead_MiscMaster_ClosureTypeId",
                schema: "Sales",
                table: "SalesLead",
                column: "ClosureTypeId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesLead_MiscMaster_ConvertWonLeadToId",
                schema: "Sales",
                table: "SalesLead",
                column: "ConvertWonLeadToId",
                principalSchema: "Sales",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalesLead_MiscMaster_ClosureReasonId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesLead_MiscMaster_ClosureTypeId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesLead_MiscMaster_ConvertWonLeadToId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropIndex(
                name: "IX_SalesLead_ClosureReasonId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropIndex(
                name: "IX_SalesLead_ClosureTypeId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropIndex(
                name: "IX_SalesLead_ConvertWonLeadToId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropColumn(
                name: "ClosureDate",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropColumn(
                name: "ClosureReasonId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropColumn(
                name: "ClosureRemarks",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropColumn(
                name: "ClosureTypeId",
                schema: "Sales",
                table: "SalesLead");

            migrationBuilder.DropColumn(
                name: "ConvertWonLeadToId",
                schema: "Sales",
                table: "SalesLead");
        }
    }
}
