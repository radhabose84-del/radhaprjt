using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateInward_InvoiceDc_ReceivingWarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DcDate",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DcNo",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InvoiceDate",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNo",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceivingWarehouseId",
                schema: "Gate",
                table: "GateInwardHdr",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_GateInwardHdr_ReceivingWarehouseId",
                schema: "Gate",
                table: "GateInwardHdr",
                column: "ReceivingWarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GateInwardHdr_ReceivingWarehouseId",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "DcDate",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "DcNo",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "InvoiceDate",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "InvoiceNo",
                schema: "Gate",
                table: "GateInwardHdr");

            migrationBuilder.DropColumn(
                name: "ReceivingWarehouseId",
                schema: "Gate",
                table: "GateInwardHdr");
        }
    }
}
