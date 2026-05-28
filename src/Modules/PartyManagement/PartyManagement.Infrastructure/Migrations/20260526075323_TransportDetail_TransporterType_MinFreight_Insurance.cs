using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransportDetail_TransporterType_MinFreight_Insurance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InsuranceExpiryDate",
                schema: "Party",
                table: "TransportDetail",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceProvider",
                schema: "Party",
                table: "TransportDetail",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinFreightAmount",
                schema: "Party",
                table: "TransportDetail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PolicyNo",
                schema: "Party",
                table: "TransportDetail",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransporterTypeId",
                schema: "Party",
                table: "TransportDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransportDetail_TransporterTypeId",
                schema: "Party",
                table: "TransportDetail",
                column: "TransporterTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TransportDetail_MiscMaster_TransporterTypeId",
                schema: "Party",
                table: "TransportDetail",
                column: "TransporterTypeId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransportDetail_MiscMaster_TransporterTypeId",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropIndex(
                name: "IX_TransportDetail_TransporterTypeId",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropColumn(
                name: "InsuranceExpiryDate",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropColumn(
                name: "InsuranceProvider",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropColumn(
                name: "MinFreightAmount",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropColumn(
                name: "PolicyNo",
                schema: "Party",
                table: "TransportDetail");

            migrationBuilder.DropColumn(
                name: "TransporterTypeId",
                schema: "Party",
                table: "TransportDetail");
        }
    }
}
