using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateEntryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VehicleMovementRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleMovementRecord",
                schema: "Gate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleMovementId = table.Column<string>(type: "varchar(20)", nullable: false),
                    VehicleNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    DriverName = table.Column<string>(type: "varchar(50)", nullable: false),
                    DriverLicenseNo = table.Column<string>(type: "varchar(25)", nullable: true),
                    DriverMobileNo = table.Column<string>(type: "varchar(10)", nullable: false),
                    TransporterId = table.Column<int>(type: "int", nullable: true),
                    PurposeOfVisitId = table.Column<int>(type: "int", nullable: false),
                    ReferenceDocTypeId = table.Column<int>(type: "int", nullable: true),
                    ReferenceDocNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    GateInTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GateInBy = table.Column<string>(type: "varchar(50)", nullable: false),
                    GateOutTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GateOutBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_VehicleMovementRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleMovementRecord_MiscMaster_PurposeOfVisitId",
                        column: x => x.PurposeOfVisitId,
                        principalSchema: "Gate",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleMovementRecord_MiscMaster_ReferenceDocTypeId",
                        column: x => x.ReferenceDocTypeId,
                        principalSchema: "Gate",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleMovementRecord_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Gate",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMovementRecord_PurposeOfVisitId",
                schema: "Gate",
                table: "VehicleMovementRecord",
                column: "PurposeOfVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMovementRecord_ReferenceDocTypeId",
                schema: "Gate",
                table: "VehicleMovementRecord",
                column: "ReferenceDocTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMovementRecord_StatusId",
                schema: "Gate",
                table: "VehicleMovementRecord",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMovementRecord_UnitId",
                schema: "Gate",
                table: "VehicleMovementRecord",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMovementRecord_VehicleMovementId",
                schema: "Gate",
                table: "VehicleMovementRecord",
                column: "VehicleMovementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMovementRecord_VehicleNumber",
                schema: "Gate",
                table: "VehicleMovementRecord",
                column: "VehicleNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleMovementRecord",
                schema: "Gate");
        }
    }
}
