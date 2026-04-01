using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class YarnConversionHeader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "YarnConversionHeader",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ProductionYear = table.Column<int>(type: "int", nullable: false),
                    ConversionDocNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    ConversionDate = table.Column<DateTime>(type: "date", nullable: false),
                    OldItemId = table.Column<int>(type: "int", nullable: false),
                    OldPackTypeId = table.Column<int>(type: "int", nullable: false),
                    OldStartPackNo = table.Column<int>(type: "int", nullable: false),
                    OldEndPackNo = table.Column<int>(type: "int", nullable: false),
                    OldTotalBags = table.Column<int>(type: "int", nullable: false),
                    OldNetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldNetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldWarehouseId = table.Column<int>(type: "int", nullable: false),
                    OldBinId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    LooseQty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    LooseHandlingId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    WasteTypeId = table.Column<int>(type: "int", nullable: true),
                    WasteQty = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0m),
                    WasteReason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    FaultId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_YarnConversionHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YarnConversionHeader_LotMaster_LotId",
                        column: x => x.LotId,
                        principalSchema: "Production",
                        principalTable: "LotMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YarnConversionHeader_MiscMaster_FaultId",
                        column: x => x.FaultId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YarnConversionHeader_MiscMaster_LooseHandlingId",
                        column: x => x.LooseHandlingId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YarnConversionHeader_MiscMaster_WasteTypeId",
                        column: x => x.WasteTypeId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YarnConversionHeader_PackType_OldPackTypeId",
                        column: x => x.OldPackTypeId,
                        principalSchema: "Production",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YarnConversionHeader_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Production",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_ConversionDate",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "ConversionDate");

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_ConversionDocNo",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "ConversionDocNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_FaultId",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "FaultId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_ItemId",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_LooseHandlingId",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "LooseHandlingId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_LotId",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_OldItemId",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "OldItemId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_OldPackTypeId",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "OldPackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_PackTypeId",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "PackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_YarnConversionHeader_WasteTypeId",
                schema: "Production",
                table: "YarnConversionHeader",
                column: "WasteTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YarnConversionHeader",
                schema: "Production");
        }
    }
}
