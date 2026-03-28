using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RepackingMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RepackingHeader",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ProductionYear = table.Column<int>(type: "int", nullable: false),
                    RepackingNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    RepackingDate = table.Column<DateTime>(type: "date", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    LooseConeKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldPackHeaderId = table.Column<int>(type: "int", nullable: false),
                    LooseHandlingId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_RepackingHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepackingHeader_MiscMaster_LooseHandlingId",
                        column: x => x.LooseHandlingId,
                        principalSchema: "Production",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingHeader_ProductionPackHeader_OldPackHeaderId",
                        column: x => x.OldPackHeaderId,
                        principalSchema: "Production",
                        principalTable: "ProductionPackHeader",
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
                    RepackingHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldPackDetailId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepackingDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepackingDetail_LotMaster_LotId",
                        column: x => x.LotId,
                        principalSchema: "Production",
                        principalTable: "LotMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingDetail_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Production",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingDetail_ProductionPackDetail_OldPackDetailId",
                        column: x => x.OldPackDetailId,
                        principalSchema: "Production",
                        principalTable: "ProductionPackDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RepackingDetail_RepackingHeader_RepackingHeaderId",
                        column: x => x.RepackingHeaderId,
                        principalSchema: "Production",
                        principalTable: "RepackingHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RepackingDetail_LotId",
                schema: "Production",
                table: "RepackingDetail",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingDetail_OldPackDetailId",
                schema: "Production",
                table: "RepackingDetail",
                column: "OldPackDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingDetail_PackTypeId",
                schema: "Production",
                table: "RepackingDetail",
                column: "PackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingDetail_RepackingHeaderId",
                schema: "Production",
                table: "RepackingDetail",
                column: "RepackingHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_LooseHandlingId",
                schema: "Production",
                table: "RepackingHeader",
                column: "LooseHandlingId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_OldPackHeaderId",
                schema: "Production",
                table: "RepackingHeader",
                column: "OldPackHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_RepackingDate",
                schema: "Production",
                table: "RepackingHeader",
                column: "RepackingDate");

            migrationBuilder.CreateIndex(
                name: "IX_RepackingHeader_RepackingNo",
                schema: "Production",
                table: "RepackingHeader",
                column: "RepackingNo",
                unique: true);
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
        }
    }
}
