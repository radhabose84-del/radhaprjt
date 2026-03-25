using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GatePassHdrAndDtl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GatePassHdr",
                schema: "Gate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GatePassNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    GatePassDate = table.Column<DateOnly>(type: "date", nullable: false),
                    VehicleMovementRecordId = table.Column<int>(type: "int", nullable: false),
                    VehicleNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    DriverName = table.Column<string>(type: "varchar(50)", nullable: true),
                    DriverMobile = table.Column<string>(type: "varchar(10)", nullable: true),
                    TransporterName = table.Column<string>(type: "varchar(100)", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    TotalItems = table.Column<int>(type: "int", nullable: false),
                    TotalDocumentQty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalDispatchQty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ReturnableItems = table.Column<int>(type: "int", nullable: true),
                    TotalValue = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatePassHdr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatePassHdr_VehicleMovementRecord_VehicleMovementRecordId",
                        column: x => x.VehicleMovementRecordId,
                        principalSchema: "Gate",
                        principalTable: "VehicleMovementRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GatePassDtl",
                schema: "Gate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GatePassHdrId = table.Column<int>(type: "int", nullable: false),
                    DocTypeId = table.Column<int>(type: "int", nullable: false),
                    DocNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    PartyName = table.Column<string>(type: "varchar(100)", nullable: true),
                    PartyCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    DocDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalQty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatePassDtl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GatePassDtl_GatePassHdr_GatePassHdrId",
                        column: x => x.GatePassHdrId,
                        principalSchema: "Gate",
                        principalTable: "GatePassHdr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GatePassDtl_GatePassHdrId",
                schema: "Gate",
                table: "GatePassDtl",
                column: "GatePassHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_GatePassHdr_GatePassNo",
                schema: "Gate",
                table: "GatePassHdr",
                column: "GatePassNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GatePassHdr_UnitId",
                schema: "Gate",
                table: "GatePassHdr",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_GatePassHdr_VehicleMovementRecordId",
                schema: "Gate",
                table: "GatePassHdr",
                column: "VehicleMovementRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GatePassDtl",
                schema: "Gate");

            migrationBuilder.DropTable(
                name: "GatePassHdr",
                schema: "Gate");
        }
    }
}
