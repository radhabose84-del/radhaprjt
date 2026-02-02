using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuotationEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuotationHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    ValidTill = table.Column<DateOnly>(type: "date", nullable: false),
                    QuotationNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FreightModeId = table.Column<int>(type: "int", nullable: false),
                    Freight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false),
                    IncotermsId = table.Column<int>(type: "int", nullable: false),
                    TaxableSubtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GstTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ItemsTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuotationImage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MiscMasterId = table.Column<int>(type: "int", nullable: true),
                    MiscMasterId1 = table.Column<int>(type: "int", nullable: true),
                    MiscMasterId2 = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_QuotationHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationHeader_MiscMaster_FreightModeId",
                        column: x => x.FreightModeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationHeader_MiscMaster_IncotermsId",
                        column: x => x.IncotermsId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationHeader_MiscMaster_MiscMasterId",
                        column: x => x.MiscMasterId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuotationHeader_MiscMaster_MiscMasterId1",
                        column: x => x.MiscMasterId1,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuotationHeader_MiscMaster_MiscMasterId2",
                        column: x => x.MiscMasterId2,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuotationHeader_MiscMaster_PaymentTermsId",
                        column: x => x.PaymentTermsId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GstPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Warranty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineSubtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GstAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_QuotationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationDetail_QuotationHeader_QuotationHeaderId",
                        column: x => x.QuotationHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "QuotationHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationDetail_Header_Item_NotDeleted",
                schema: "Purchase",
                table: "QuotationDetail",
                columns: new[] { "QuotationHeaderId", "ItemId", "IsDeleted" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Purchase_QuotationHeader_QuotationNumber",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "QuotationNumber")
                .Annotation("SqlServer:Include", new[] { "SupplierId", "RfqId", "ValidTill" });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_FreightModeId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "FreightModeId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_IncotermsId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "IncotermsId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_MiscMasterId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_MiscMasterId1",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId1");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_MiscMasterId2",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "MiscMasterId2");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_PaymentTermsId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "PaymentTermsId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationHeader_StatusId",
                schema: "Purchase",
                table: "QuotationHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UQ_Quotation_Supplier_Rfq_NotDeleted",
                schema: "Purchase",
                table: "QuotationHeader",
                columns: new[] { "SupplierId", "RfqId", "IsDeleted" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "QuotationHeader",
                schema: "Purchase");
        }
    }
}
