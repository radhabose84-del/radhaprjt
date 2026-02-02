using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Itemcource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceMasterHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    ValidFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsApprove = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)0),
                    SourceFrom = table.Column<int>(type: "int", nullable: false),
                    SourceDetailId = table.Column<int>(type: "int", nullable: false),
                    MiscMasterId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceMasterHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceMasterHeader_MiscMaster_MiscMasterId",
                        column: x => x.MiscMasterId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PriceMasterHeader_MiscMaster_SourceFrom",
                        column: x => x.SourceFrom,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceMasterDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PriceMasterHeaderId = table.Column<int>(type: "int", nullable: false),
                    ScaleQtyFrom = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ScaleQtyTo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceMasterDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceMasterDetail_PriceMasterHeader_PriceMasterHeaderId",
                        column: x => x.PriceMasterHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "PriceMasterHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterDetail_PriceMasterHeaderId_ScaleQtyFrom",
                schema: "Purchase",
                table: "PriceMasterDetail",
                columns: new[] { "PriceMasterHeaderId", "ScaleQtyFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidFrom",
                schema: "Purchase",
                table: "PriceMasterHeader",
                columns: new[] { "ItemId", "VendorId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_ItemId_VendorId_ValidTo",
                schema: "Purchase",
                table: "PriceMasterHeader",
                columns: new[] { "ItemId", "VendorId", "ValidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_MiscMasterId",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceMasterHeader_SourceFrom",
                schema: "Purchase",
                table: "PriceMasterHeader",
                column: "SourceFrom");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceMasterDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PriceMasterHeader",
                schema: "Purchase");
        }
    }
}
