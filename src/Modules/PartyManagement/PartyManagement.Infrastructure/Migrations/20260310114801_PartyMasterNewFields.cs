using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartyMasterNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DefaultFreightRate",
                schema: "Party",
                table: "PartyMaster",
                type: "decimal(18,3)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DefaultFreightTypeId",
                schema: "Party",
                table: "PartyMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FreightExpensesGl",
                schema: "Party",
                table: "PartyMaster",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LicenseExpiryDate",
                schema: "Party",
                table: "PartyMaster",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNo",
                schema: "Party",
                table: "PartyMaster",
                type: "nvarchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportModeId",
                schema: "Party",
                table: "PartyMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleTypeId",
                schema: "Party",
                table: "PartyMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_DefaultFreightTypeId",
                schema: "Party",
                table: "PartyMaster",
                column: "DefaultFreightTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_TransportModeId",
                schema: "Party",
                table: "PartyMaster",
                column: "TransportModeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyMaster_VehicleTypeId",
                schema: "Party",
                table: "PartyMaster",
                column: "VehicleTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PartyMaster_MiscMaster_DefaultFreightTypeId",
                schema: "Party",
                table: "PartyMaster",
                column: "DefaultFreightTypeId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartyMaster_MiscMaster_TransportModeId",
                schema: "Party",
                table: "PartyMaster",
                column: "TransportModeId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartyMaster_MiscMaster_VehicleTypeId",
                schema: "Party",
                table: "PartyMaster",
                column: "VehicleTypeId",
                principalSchema: "Party",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartyMaster_MiscMaster_DefaultFreightTypeId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_PartyMaster_MiscMaster_TransportModeId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_PartyMaster_MiscMaster_VehicleTypeId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropIndex(
                name: "IX_PartyMaster_DefaultFreightTypeId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropIndex(
                name: "IX_PartyMaster_TransportModeId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropIndex(
                name: "IX_PartyMaster_VehicleTypeId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "DefaultFreightRate",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "DefaultFreightTypeId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "FreightExpensesGl",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "LicenseExpiryDate",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "LicenseNo",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "TransportModeId",
                schema: "Party",
                table: "PartyMaster");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                schema: "Party",
                table: "PartyMaster");
        }
    }
}
