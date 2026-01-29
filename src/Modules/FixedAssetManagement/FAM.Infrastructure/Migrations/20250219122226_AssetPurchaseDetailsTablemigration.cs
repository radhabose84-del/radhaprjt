using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetPurchaseDetailsTablemigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetPurchaseDetails",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetType = table.Column<string>(type: "varchar(50)", nullable: true),
                    OldUnitId = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    VendorCode = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    VendorName = table.Column<string>(type: "nvarchar(200)", nullable: false),
                    PoDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PoNo = table.Column<int>(type: "int", nullable: false),
                    PoSno = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(250)", nullable: false),
                    GrnNo = table.Column<int>(type: "int", nullable: false),
                    GrnSno = table.Column<int>(type: "int", nullable: false),
                    GrnDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    QcCompleted = table.Column<string>(type: "char(1)", nullable: false),
                    AcceptedQty = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PurchaseValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    GrnValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    BillNo = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    BillDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Uom = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    BinLocation = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    PjYear = table.Column<string>(type: "varchar(8)", nullable: false),
                    PjDocId = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    PjDocSr = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    PjDocNo = table.Column<int>(type: "int", nullable: false),
                    AssetMasterId = table.Column<int>(type: "int", nullable: false),
                    AssetSourceId = table.Column<int>(type: "int", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    CapitalizationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetPurchaseDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetPurchaseDetails_AssetMaster_AssetMasterId",
                        column: x => x.AssetMasterId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetPurchaseDetails_AssetSource_AssetSourceId",
                        column: x => x.AssetSourceId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetPurchaseDetails_UOM_UOMId",
                        column: x => x.UOMId,
                        principalSchema: "FixedAsset",
                        principalTable: "UOM",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetPurchaseDetails_AssetMasterId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "AssetMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetPurchaseDetails_AssetSourceId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "AssetSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetPurchaseDetails_UOMId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "UOMId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetPurchaseDetails",
                schema: "FixedAsset");
        }
    }
}
