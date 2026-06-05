using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RawMaterialPO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RawMaterialPOHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    PONumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    PODate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    OcrId = table.Column<int>(type: "int", nullable: false),
                    ProcurementDocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    TaxableTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalGstAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NetTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("PK_RawMaterialPOHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawMaterialPOHeader_MiscMaster_ProcurementDocumentTypeId",
                        column: x => x.ProcurementDocumentTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterialPOHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RawMaterialPOHeader_OCREntry_OcrId",
                        column: x => x.OcrId,
                        principalSchema: "Purchase",
                        principalTable: "OCREntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RawMaterialPODetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RawMaterialPOId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CGSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    SGSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IGSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CGSTValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SGSTValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IGSTValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TotalGST = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_RawMaterialPODetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawMaterialPODetail_RawMaterialPOHeader_RawMaterialPOId",
                        column: x => x.RawMaterialPOId,
                        principalSchema: "Purchase",
                        principalTable: "RawMaterialPOHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialPODetail_HsnId",
                schema: "Purchase",
                table: "RawMaterialPODetail",
                column: "HsnId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialPODetail_ItemId",
                schema: "Purchase",
                table: "RawMaterialPODetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialPODetail_RawMaterialPOId",
                schema: "Purchase",
                table: "RawMaterialPODetail",
                column: "RawMaterialPOId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialPOHeader_OcrId",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                column: "OcrId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialPOHeader_PONumber",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                column: "PONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialPOHeader_ProcurementDocumentTypeId",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                column: "ProcurementDocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialPOHeader_StatusId",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RawMaterialPOHeader_UnitId",
                schema: "Purchase",
                table: "RawMaterialPOHeader",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawMaterialPODetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "RawMaterialPOHeader",
                schema: "Purchase");
        }
    }
}
