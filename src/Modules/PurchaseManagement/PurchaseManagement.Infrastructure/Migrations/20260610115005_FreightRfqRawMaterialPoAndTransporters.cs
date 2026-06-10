using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FreightRfqRawMaterialPoAndTransporters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FreightRfqHeader_PurchaseOrder",
                schema: "Purchase",
                table: "FreightRfqHeader");

            migrationBuilder.AlterColumn<int>(
                name: "RateBasisId",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "QuotedRate",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "decimal(18,2)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AlterColumn<decimal>(
                name: "FreightValue",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "decimal(18,2)",
                precision: 18,
                scale: 6,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 6);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "NotifiedDate",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportDetailId",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransportModeName",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleNo",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VehicleTypeName",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RfqValidTill",
                schema: "Purchase",
                table: "FreightRfqHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FreightRfqHeader_RawMaterialPO",
                schema: "Purchase",
                table: "FreightRfqHeader",
                column: "PoReferenceId",
                principalSchema: "Purchase",
                principalTable: "RawMaterialPOHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FreightRfqHeader_RawMaterialPO",
                schema: "Purchase",
                table: "FreightRfqHeader");

            migrationBuilder.DropColumn(
                name: "NotifiedDate",
                schema: "Purchase",
                table: "FreightRfqQuotation");

            migrationBuilder.DropColumn(
                name: "TransportDetailId",
                schema: "Purchase",
                table: "FreightRfqQuotation");

            migrationBuilder.DropColumn(
                name: "TransportModeName",
                schema: "Purchase",
                table: "FreightRfqQuotation");

            migrationBuilder.DropColumn(
                name: "VehicleNo",
                schema: "Purchase",
                table: "FreightRfqQuotation");

            migrationBuilder.DropColumn(
                name: "VehicleTypeName",
                schema: "Purchase",
                table: "FreightRfqQuotation");

            migrationBuilder.DropColumn(
                name: "RfqValidTill",
                schema: "Purchase",
                table: "FreightRfqHeader");

            migrationBuilder.AlterColumn<int>(
                name: "RateBasisId",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "QuotedRate",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "decimal(18,2)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "FreightValue",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                type: "decimal(18,2)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 6,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FreightRfqHeader_PurchaseOrder",
                schema: "Purchase",
                table: "FreightRfqHeader",
                column: "PoReferenceId",
                principalSchema: "Purchase",
                principalTable: "PurchaseOrderHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
