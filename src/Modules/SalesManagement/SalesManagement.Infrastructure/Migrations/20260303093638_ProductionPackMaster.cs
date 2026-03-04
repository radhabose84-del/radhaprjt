using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductionPackMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionDetail",
                schema: "Production");

            migrationBuilder.DropTable(
                name: "ProductionHeader",
                schema: "Production");

            migrationBuilder.CreateTable(
                name: "ProductionPackHeader",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackNo = table.Column<string>(type: "varchar(30)", nullable: false),
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
                    table.PrimaryKey("PK_ProductionPackHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionPackHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionPackDetail",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionPackHeaderId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ProductionPackDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionPackDetail_LotMaster_LotId",
                        column: x => x.LotId,
                        principalSchema: "Sales",
                        principalTable: "LotMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionPackDetail_MiscMaster_QualityStatusId",
                        column: x => x.QualityStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionPackDetail_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Sales",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionPackDetail_ProductionPackHeader_ProductionPackHeaderId",
                        column: x => x.ProductionPackHeaderId,
                        principalSchema: "Production",
                        principalTable: "ProductionPackHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_BinId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_LotId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_PackTypeId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_ProductionPackHeaderId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "ProductionPackHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_QualityStatusId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "QualityStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_PackDate",
                schema: "Production",
                table: "ProductionPackHeader",
                column: "PackDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_PackNo",
                schema: "Production",
                table: "ProductionPackHeader",
                column: "PackNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_StatusId",
                schema: "Production",
                table: "ProductionPackHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_WarehouseId",
                schema: "Production",
                table: "ProductionPackHeader",
                column: "WarehouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionPackDetail",
                schema: "Production");

            migrationBuilder.DropTable(
                name: "ProductionPackHeader",
                schema: "Production");

            migrationBuilder.CreateTable(
                name: "ProductionHeader",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    PackDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ProductionNo = table.Column<string>(type: "varchar(30)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalNetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionDetail",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    ProductionHeaderId = table.Column<int>(type: "int", nullable: false),
                    QualityStatusId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    LineRemarks = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalNetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionDetail_LotMaster_LotId",
                        column: x => x.LotId,
                        principalSchema: "Sales",
                        principalTable: "LotMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionDetail_MiscMaster_QualityStatusId",
                        column: x => x.QualityStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionDetail_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Sales",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionDetail_ProductionHeader_ProductionHeaderId",
                        column: x => x.ProductionHeaderId,
                        principalSchema: "Production",
                        principalTable: "ProductionHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetail_BinId",
                schema: "Production",
                table: "ProductionDetail",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetail_LotId",
                schema: "Production",
                table: "ProductionDetail",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetail_PackTypeId",
                schema: "Production",
                table: "ProductionDetail",
                column: "PackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetail_ProductionHeaderId",
                schema: "Production",
                table: "ProductionDetail",
                column: "ProductionHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionDetail_QualityStatusId",
                schema: "Production",
                table: "ProductionDetail",
                column: "QualityStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHeader_PackDate",
                schema: "Production",
                table: "ProductionHeader",
                column: "PackDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHeader_ProductionNo",
                schema: "Production",
                table: "ProductionHeader",
                column: "ProductionNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHeader_StatusId",
                schema: "Production",
                table: "ProductionHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionHeader_WarehouseId",
                schema: "Production",
                table: "ProductionHeader",
                column: "WarehouseId");
        }
    }
}
