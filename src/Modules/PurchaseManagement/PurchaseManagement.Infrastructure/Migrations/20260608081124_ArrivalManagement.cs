using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ArrivalManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArrivalHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ArrivalNumber = table.Column<string>(type: "varchar(30)", nullable: false),
                    ArrivalDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RawMaterialPOId = table.Column<int>(type: "int", nullable: false),
                    VehicleNumber = table.Column<string>(type: "varchar(30)", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    GodownId = table.Column<int>(type: "int", nullable: false),
                    TransporterId = table.Column<int>(type: "int", nullable: false),
                    FreightRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    InvoiceGstNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    LrNumber = table.Column<string>(type: "varchar(30)", nullable: true),
                    ContainerNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    LorryIn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LorryOut = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TareWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PartyWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WeightDifference = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MoisturePercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    QcStatusId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrivalHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArrivalHeader_MiscMaster_QcStatusId",
                        column: x => x.QcStatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArrivalHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArrivalHeader_RawMaterialPOHeader_RawMaterialPOId",
                        column: x => x.RawMaterialPOId,
                        principalSchema: "Purchase",
                        principalTable: "RawMaterialPOHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockLedgerRaw",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    LotNo = table.Column<int>(type: "int", nullable: false),
                    BaleNo = table.Column<long>(type: "bigint", nullable: false),
                    BarcodeNumber = table.Column<long>(type: "bigint", nullable: false),
                    BaleWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<string>(type: "varchar(3)", nullable: false, defaultValue: "ARV"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLedgerRaw", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArrivalDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ArrivalHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    MixCodeId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ArrivedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CancelledQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalanceQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BatchNumber = table.Column<string>(type: "varchar(30)", nullable: false),
                    BaleNumberFrom = table.Column<long>(type: "bigint", nullable: false),
                    BaleNumberTo = table.Column<long>(type: "bigint", nullable: false),
                    TotalBaleCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrivalDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArrivalDetail_ArrivalHeader_ArrivalHeaderId",
                        column: x => x.ArrivalHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "ArrivalHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalDetail_ArrivalHeaderId",
                schema: "Purchase",
                table: "ArrivalDetail",
                column: "ArrivalHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalDetail_ItemId",
                schema: "Purchase",
                table: "ArrivalDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalHeader_ArrivalNumber",
                schema: "Purchase",
                table: "ArrivalHeader",
                column: "ArrivalNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalHeader_QcStatusId",
                schema: "Purchase",
                table: "ArrivalHeader",
                column: "QcStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalHeader_RawMaterialPOId",
                schema: "Purchase",
                table: "ArrivalHeader",
                column: "RawMaterialPOId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalHeader_StatusId",
                schema: "Purchase",
                table: "ArrivalHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrivalHeader_UnitId",
                schema: "Purchase",
                table: "ArrivalHeader",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedgerRaw_BarcodeNumber",
                schema: "Purchase",
                table: "StockLedgerRaw",
                column: "BarcodeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedgerRaw_ItemId",
                schema: "Purchase",
                table: "StockLedgerRaw",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedgerRaw_LotNo",
                schema: "Purchase",
                table: "StockLedgerRaw",
                column: "LotNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArrivalDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "StockLedgerRaw",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "ArrivalHeader",
                schema: "Purchase");
        }
    }
}
