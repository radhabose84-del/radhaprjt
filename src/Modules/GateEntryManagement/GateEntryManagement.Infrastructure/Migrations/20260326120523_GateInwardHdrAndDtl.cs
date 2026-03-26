using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GateInwardHdrAndDtl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GateInwardHdr",
                schema: "Gate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateEntryNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    VehicleMovementRecordId = table.Column<int>(type: "int", nullable: false),
                    GrossWeight = table.Column<decimal>(type: "decimal(10,3)", precision: 18, scale: 6, nullable: true),
                    TareWeight = table.Column<decimal>(type: "decimal(10,3)", precision: 18, scale: 6, nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(10,3)", precision: 18, scale: 6, nullable: true),
                    QAInspectionRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    QAStatusId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(250)", nullable: true),
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
                    table.PrimaryKey("PK_GateInwardHdr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateInwardHdr_MiscMaster_QAStatusId",
                        column: x => x.QAStatusId,
                        principalSchema: "Gate",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GateInwardHdr_VehicleMovementRecord_VehicleMovementRecordId",
                        column: x => x.VehicleMovementRecordId,
                        principalSchema: "Gate",
                        principalTable: "VehicleMovementRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GateInwardDtl",
                schema: "Gate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateInwardHdrId = table.Column<int>(type: "int", nullable: false),
                    ReferenceDocTypeId = table.Column<int>(type: "int", nullable: false),
                    ReferenceDocNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    PartyName = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateInwardDtl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateInwardDtl_GateInwardHdr_GateInwardHdrId",
                        column: x => x.GateInwardHdrId,
                        principalSchema: "Gate",
                        principalTable: "GateInwardHdr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GateInwardDtl_GateInwardHdrId",
                schema: "Gate",
                table: "GateInwardDtl",
                column: "GateInwardHdrId");

            migrationBuilder.CreateIndex(
                name: "IX_GateInwardHdr_GateEntryNo",
                schema: "Gate",
                table: "GateInwardHdr",
                column: "GateEntryNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GateInwardHdr_QAStatusId",
                schema: "Gate",
                table: "GateInwardHdr",
                column: "QAStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_GateInwardHdr_UnitId",
                schema: "Gate",
                table: "GateInwardHdr",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_GateInwardHdr_VehicleMovementRecordId",
                schema: "Gate",
                table: "GateInwardHdr",
                column: "VehicleMovementRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GateInwardDtl",
                schema: "Gate");

            migrationBuilder.DropTable(
                name: "GateInwardHdr",
                schema: "Gate");
        }
    }
}
