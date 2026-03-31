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
                name: "RepackingMaster",
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
                    OldPackTypeId = table.Column<int>(type: "int", nullable: false),
                    OldNetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldStartPackNo = table.Column<int>(type: "int", nullable: false),
                    OldEndPackNo = table.Column<int>(type: "int", nullable: false),
                    OldTotalBags = table.Column<int>(type: "int", nullable: false),
                    OldNetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    OldWarehouseId = table.Column<int>(type: "int", nullable: false),
                    OldBinId = table.Column<int>(type: "int", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    LooseConeKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RepackingMaster",
                schema: "Production");
        }
    }
}
