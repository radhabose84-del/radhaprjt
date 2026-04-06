using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RepackingHeaderMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepackingMaster",
                schema: "Production");

            migrationBuilder.DropTable(
                name: "YarnConversionHeader",
                schema: "Production");

            migrationBuilder.CreateTable(
                name: "RepackingHeader",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ProductionYear = table.Column<int>(type: "int", nullable: false),
                    RepackDocNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    RepackDate = table.Column<DateTime>(type: "date", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    OldItemId = table.Column<int>(type: "int", nullable: false),
                    OldPackTypeId = table.Column<int>(type: "int", nullable: false),
                    LooseConeKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    LooseHandlingId = table.Column<int>(type: "int", nullable: true),
                    FaultId = table.Column<int>(type: "int", nullable: true),
                    WasteTypeId = table.Column<int>(type: "int", nullable: true),
                    WasteQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0m),
                    WasteReason = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_RepackingHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepackingHeader_LotMaster_LotId",
                        column: x => x.LotId,
                        principalSchema: "Production",
                        principalTable: "LotMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingHeader_MiscMaster_FaultId",
                        column: x => x.FaultId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingHeader_MiscMaster_LooseHandlingId",
                        column: x => x.LooseHandlingId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingHeader_MiscMaster_WasteTypeId",
                        column: x => x.WasteTypeId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingHeader_PackType_OldPackTypeId",
                        column: x => x.OldPackTypeId,
                        principalSchema: "Production",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingHeader_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Production",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RepackingDetail",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RepackHeaderId = table.Column<int>(type: "int", nullable: false),
                    OldStartPackNo = table.Column<int>(type: "int", nullable: false),
                    OldEndPackNo = table.Column<int>(type: "int", nullable: false),
                    OldNetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldTotalBags = table.Column<int>(type: "int", nullable: false),
                    OldNetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldWarehouseId = table.Column<int>(type: "int", nullable: false),
                    OldBinId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepackingDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepackingDetail_RepackingHeader_RepackHeaderId",
                        column: x => x.RepackHeaderId,
                        principalSchema: "Production",
                        principalTable: "RepackingHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepackingDetail_OldBinId",
                schema: "Production",
                table: "RepackingDetail",
                column: "OldBinId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingDetail_OldWarehouseId",
                schema: "Production",
                table: "RepackingDetail",
                column: "OldWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingDetail_RepackHeaderId",
                schema: "Production",
                table: "RepackingDetail",
                column: "RepackHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_BinId",
                schema: "Production",
                table: "RepackingHeader",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_FaultId",
                schema: "Production",
                table: "RepackingHeader",
                column: "FaultId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_ItemId",
                schema: "Production",
                table: "RepackingHeader",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_LooseHandlingId",
                schema: "Production",
                table: "RepackingHeader",
                column: "LooseHandlingId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_LotId",
                schema: "Production",
                table: "RepackingHeader",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_OldItemId",
                schema: "Production",
                table: "RepackingHeader",
                column: "OldItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_OldPackTypeId",
                schema: "Production",
                table: "RepackingHeader",
                column: "OldPackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_PackTypeId",
                schema: "Production",
                table: "RepackingHeader",
                column: "PackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_RepackDate",
                schema: "Production",
                table: "RepackingHeader",
                column: "RepackDate");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_RepackDocNo",
                schema: "Production",
                table: "RepackingHeader",
                column: "RepackDocNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_WarehouseId",
                schema: "Production",
                table: "RepackingHeader",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_WasteTypeId",
                schema: "Production",
                table: "RepackingHeader",
                column: "WasteTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepackingDetail",
                schema: "Production");

            migrationBuilder.DropTable(
                name: "RepackingHeader",
                schema: "Production");

            migrationBuilder.CreateTable(
                name: "RepackingMaster",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LooseHandlingId = table.Column<int>(type: "int", nullable: true),
                    OldPackTypeId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LooseConeKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldBinId = table.Column<int>(type: "int", nullable: false),
                    OldEndPackNo = table.Column<int>(type: "int", nullable: false),
                    OldNetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldNetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldStartPackNo = table.Column<int>(type: "int", nullable: false),
                    OldTotalBags = table.Column<int>(type: "int", nullable: false),
                    OldWarehouseId = table.Column<int>(type: "int", nullable: false),
                    ProductionYear = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    RepackDate = table.Column<DateTime>(type: "date", nullable: false),
                    RepackDocNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepackingMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepackingMaster_MiscMaster_LooseHandlingId",
                        column: x => x.LooseHandlingId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingMaster_PackType_OldPackTypeId",
                        column: x => x.OldPackTypeId,
                        principalSchema: "Production",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingMaster_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Production",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YarnConversionHeader",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaultId = table.Column<int>(type: "int", nullable: true),
                    LooseHandlingId = table.Column<int>(type: "int", nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    OldPackTypeId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    WasteTypeId = table.Column<int>(type: "int", nullable: true),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    ConversionDate = table.Column<DateTime>(type: "date", nullable: false),
                    ConversionDocNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LooseQty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldBinId = table.Column<int>(type: "int", nullable: false),
                    OldEndPackNo = table.Column<int>(type: "int", nullable: false),
                    OldItemId = table.Column<int>(type: "int", nullable: false),
                    OldNetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldNetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldStartPackNo = table.Column<int>(type: "int", nullable: false),
                    OldTotalBags = table.Column<int>(type: "int", nullable: false),
                    OldWarehouseId = table.Column<int>(type: "int", nullable: false),
                    ProductionYear = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    WasteQty = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0m),
                    WasteReason = table.Column<string>(type: "nvarchar(500)", nullable: true)
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
                name: "IX_RepackingMaster_ItemId",
                schema: "Production",
                table: "RepackingMaster",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingMaster_LooseHandlingId",
                schema: "Production",
                table: "RepackingMaster",
                column: "LooseHandlingId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingMaster_OldPackTypeId",
                schema: "Production",
                table: "RepackingMaster",
                column: "OldPackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingMaster_PackTypeId",
                schema: "Production",
                table: "RepackingMaster",
                column: "PackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingMaster_RepackDate",
                schema: "Production",
                table: "RepackingMaster",
                column: "RepackDate");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingMaster_RepackDocNo",
                schema: "Production",
                table: "RepackingMaster",
                column: "RepackDocNo",
                unique: true);

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
    }
}
