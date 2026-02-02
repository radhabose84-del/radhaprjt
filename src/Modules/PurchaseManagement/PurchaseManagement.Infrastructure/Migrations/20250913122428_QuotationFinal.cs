using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuotationFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuotationConfirmedHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqId = table.Column<int>(type: "int", nullable: false),
                    RfqCode = table.Column<string>(type: "varchar(30)", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(400)", nullable: false),
                    ConfirmedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(400)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationConfirmedHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationConfirmedHeader_RfqMaster_RfqId",
                        column: x => x.RfqId,
                        principalSchema: "Purchase",
                        principalTable: "RfqMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationConfirmedDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationConfirmedHeaderId = table.Column<int>(type: "int", nullable: false),
                    QuotationHeaderId = table.Column<int>(type: "int", nullable: false),
                    QuotationDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(400)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    Uom = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Freight = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    FreightPerItem = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    GstPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    GstAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    ValidTill = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DeliveryDays = table.Column<int>(type: "int", nullable: false),
                    Net = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandedUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(400)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationConfirmedDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationConfirmedDetail_QuotationConfirmedHeader_QuotationConfirmedHeaderId",
                        column: x => x.QuotationConfirmedHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "QuotationConfirmedHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationConfirmedDetail_QuotationDetail_QuotationDetailId",
                        column: x => x.QuotationDetailId,
                        principalSchema: "Purchase",
                        principalTable: "QuotationDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationConfirmedDetail_QuotationHeader_QuotationHeaderId",
                        column: x => x.QuotationHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "QuotationHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationConfirmedDetail_QuotationConfirmedHeaderId",
                schema: "Purchase",
                table: "QuotationConfirmedDetail",
                column: "QuotationConfirmedHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationConfirmedDetail_QuotationDetailId",
                schema: "Purchase",
                table: "QuotationConfirmedDetail",
                column: "QuotationDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationConfirmedDetail_QuotationHeaderId",
                schema: "Purchase",
                table: "QuotationConfirmedDetail",
                column: "QuotationHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationConfirmedHeader_RfqId",
                schema: "Purchase",
                table: "QuotationConfirmedHeader",
                column: "RfqId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationConfirmedDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "QuotationConfirmedHeader",
                schema: "Purchase");
        }
    }
}
