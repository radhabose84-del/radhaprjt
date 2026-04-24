using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameProductionPackDetailToEntry_AddVariantId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionPackDetail",
                schema: "Production");

            migrationBuilder.CreateTable(
                name: "ProductionPackEntry",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    PackDate = table.Column<DateTime>(type: "date", nullable: false),
                    ProductionYear = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: true),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    StartPackNo = table.Column<int>(type: "int", nullable: true),
                    EndPackNo = table.Column<int>(type: "int", nullable: true),
                    OpeningLooseKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalProductionKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalNetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ProductionKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    LooseConeKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: true),
                    QualityStatusId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    LotMasterId = table.Column<int>(type: "int", nullable: true),
                    MiscMasterId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ProductionPackEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionPackEntry_LotMaster_LotMasterId",
                        column: x => x.LotMasterId,
                        principalSchema: "Production",
                        principalTable: "LotMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionPackEntry_MiscMaster_MiscMasterId",
                        column: x => x.MiscMasterId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionPackEntry_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Production",
                        principalTable: "PackType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntry_ItemId",
                schema: "Production",
                table: "ProductionPackEntry",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntry_LotId",
                schema: "Production",
                table: "ProductionPackEntry",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntry_LotMasterId",
                schema: "Production",
                table: "ProductionPackEntry",
                column: "LotMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntry_MiscMasterId",
                schema: "Production",
                table: "ProductionPackEntry",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntry_PackDate",
                schema: "Production",
                table: "ProductionPackEntry",
                column: "PackDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntry_PackNo_ProductionYear",
                schema: "Production",
                table: "ProductionPackEntry",
                columns: new[] { "PackNo", "ProductionYear" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackEntry_PackTypeId",
                schema: "Production",
                table: "ProductionPackEntry",
                column: "PackTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionPackEntry",
                schema: "Production");

            migrationBuilder.CreateTable(
                name: "ProductionPackDetail",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BinId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    EndPackNo = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LooseConeKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    LotMasterId = table.Column<int>(type: "int", nullable: true),
                    MiscMasterId = table.Column<int>(type: "int", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    OpeningLooseKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PackDate = table.Column<DateTime>(type: "date", nullable: false),
                    PackNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    PackTypeId = table.Column<int>(type: "int", nullable: true),
                    ProductionKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ProductionYear = table.Column<int>(type: "int", nullable: false),
                    QualityStatusId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    StartPackNo = table.Column<int>(type: "int", nullable: true),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalNetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalProductionKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPackDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionPackDetail_LotMaster_LotMasterId",
                        column: x => x.LotMasterId,
                        principalSchema: "Production",
                        principalTable: "LotMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionPackDetail_MiscMaster_MiscMasterId",
                        column: x => x.MiscMasterId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductionPackDetail_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Production",
                        principalTable: "PackType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_ItemId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_LotId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_LotMasterId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "LotMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_MiscMasterId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_PackDate",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_PackNo_ProductionYear",
                schema: "Production",
                table: "ProductionPackDetail",
                columns: new[] { "PackNo", "ProductionYear" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_PackTypeId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackTypeId");
        }
    }
}
