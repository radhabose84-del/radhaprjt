using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PartyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransportDetailTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "TransportDetail",
                schema: "Party",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    TransportModeId = table.Column<int>(type: "int", nullable: true),
                    VehicleTypeId = table.Column<int>(type: "int", nullable: true),
                    DefaultFreightTypeId = table.Column<int>(type: "int", nullable: true),
                    DefaultFreightRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: true),
                    LicenseNo = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    LicenseExpiryDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    VehicleNo = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransportDetail_MiscMaster_DefaultFreightTypeId",
                        column: x => x.DefaultFreightTypeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransportDetail_MiscMaster_TransportModeId",
                        column: x => x.TransportModeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransportDetail_MiscMaster_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalSchema: "Party",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TransportDetail_PartyMaster_PartyId",
                        column: x => x.PartyId,
                        principalSchema: "Party",
                        principalTable: "PartyMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TransportDetail_DefaultFreightTypeId",
                schema: "Party",
                table: "TransportDetail",
                column: "DefaultFreightTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportDetail_PartyId",
                schema: "Party",
                table: "TransportDetail",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportDetail_TransportModeId",
                schema: "Party",
                table: "TransportDetail",
                column: "TransportModeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransportDetail_VehicleTypeId",
                schema: "Party",
                table: "TransportDetail",
                column: "VehicleTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransportDetail",
                schema: "Party");

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
    }
}
