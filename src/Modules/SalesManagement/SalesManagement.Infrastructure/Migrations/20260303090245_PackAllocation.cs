using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PackAllocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackAllocationHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackAllocationNo = table.Column<string>(type: "varchar(30)", nullable: false),
                    PackDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalNetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_PackAllocationHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackAllocationHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockLedger",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<string>(type: "varchar(10)", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocSno = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PackNo = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    TotalQty = table.Column<int>(type: "int", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLedger", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackAllocationDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackAllocationHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalNetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    QualityStatusId = table.Column<int>(type: "int", nullable: false),
                    LineRemarks = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackAllocationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackAllocationDetail_LotMaster_LotId",
                        column: x => x.LotId,
                        principalSchema: "Sales",
                        principalTable: "LotMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackAllocationDetail_MiscMaster_QualityStatusId",
                        column: x => x.QualityStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackAllocationDetail_PackAllocationHeader_PackAllocationHeaderId",
                        column: x => x.PackAllocationHeaderId,
                        principalSchema: "Sales",
                        principalTable: "PackAllocationHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackAllocationDetail_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Sales",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PackAllocationDetail_BinId",
                schema: "Sales",
                table: "PackAllocationDetail",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_PackAllocationDetail_LotId",
                schema: "Sales",
                table: "PackAllocationDetail",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_PackAllocationDetail_PackAllocationHeaderId",
                schema: "Sales",
                table: "PackAllocationDetail",
                column: "PackAllocationHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PackAllocationDetail_PackTypeId",
                schema: "Sales",
                table: "PackAllocationDetail",
                column: "PackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PackAllocationDetail_QualityStatusId",
                schema: "Sales",
                table: "PackAllocationDetail",
                column: "QualityStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PackAllocationHeader_PackAllocationNo",
                schema: "Sales",
                table: "PackAllocationHeader",
                column: "PackAllocationNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PackAllocationHeader_PackDate",
                schema: "Sales",
                table: "PackAllocationHeader",
                column: "PackDate");

            migrationBuilder.CreateIndex(
                name: "IX_PackAllocationHeader_StatusId",
                schema: "Sales",
                table: "PackAllocationHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PackAllocationHeader_WarehouseId",
                schema: "Sales",
                table: "PackAllocationHeader",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedger_DocDate",
                schema: "Sales",
                table: "StockLedger",
                column: "DocDate");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedger_DocType_DocNo_PackNo",
                schema: "Sales",
                table: "StockLedger",
                columns: new[] { "DocType", "DocNo", "PackNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockLedger_ItemId",
                schema: "Sales",
                table: "StockLedger",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockLedger_WarehouseId",
                schema: "Sales",
                table: "StockLedger",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackAllocationDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "StockLedger",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "PackAllocationHeader",
                schema: "Sales");
        }
    }
}
