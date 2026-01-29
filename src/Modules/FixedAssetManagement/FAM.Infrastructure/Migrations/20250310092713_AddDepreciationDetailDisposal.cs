using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDepreciationDetailDisposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepreciationPeriod",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DisposalAmount",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DisposedDate",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationDetail_DepreciationPeriod",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                column: "DepreciationPeriod");

            migrationBuilder.AddForeignKey(
                name: "FK_DepreciationDetail_MiscMaster_DepreciationPeriod",
                schema: "FixedAsset",
                table: "DepreciationDetail",
                column: "DepreciationPeriod",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DepreciationDetail_MiscMaster_DepreciationPeriod",
                schema: "FixedAsset",
                table: "DepreciationDetail");

            migrationBuilder.DropIndex(
                name: "IX_DepreciationDetail_DepreciationPeriod",
                schema: "FixedAsset",
                table: "DepreciationDetail");

            migrationBuilder.DropColumn(
                name: "DepreciationPeriod",
                schema: "FixedAsset",
                table: "DepreciationDetail");

            migrationBuilder.DropColumn(
                name: "DisposalAmount",
                schema: "FixedAsset",
                table: "DepreciationDetail");

            migrationBuilder.DropColumn(
                name: "DisposedDate",
                schema: "FixedAsset",
                table: "DepreciationDetail");
        }
    }
}
