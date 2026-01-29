using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class receiptdtlcolumnadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AckDate",
                schema: "FixedAsset",
                table: "AssetTransferReceiptDtl",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AckStatus",
                schema: "FixedAsset",
                table: "AssetTransferReceiptDtl",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AckDate",
                schema: "FixedAsset",
                table: "AssetTransferReceiptDtl");

            migrationBuilder.DropColumn(
                name: "AckStatus",
                schema: "FixedAsset",
                table: "AssetTransferReceiptDtl");
        }
    }
}
