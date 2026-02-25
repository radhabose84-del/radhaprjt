using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class QuotationCompareTblchnages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationConfirmedDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "QuotationConfirmedHeader",
                schema: "Purchase");

            migrationBuilder.CreateTable(
                name: "QuotationComparisonHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqId = table.Column<int>(type: "int", nullable: false),
                    RfqCode = table.Column<string>(type: "varchar(30)", nullable: false),
                    ConfirmedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationComparisonHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationComparisonHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuotationComparisonHeader_RfqMaster_RfqId",
                        column: x => x.RfqId,
                        principalSchema: "Purchase",
                        principalTable: "RfqMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QuotationComparisonDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuotationConfirmedHeaderId = table.Column<int>(type: "int", nullable: false),
                    QuotationHeaderId = table.Column<int>(type: "int", nullable: false),
                    Net = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LandedUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OverrideStatus = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(400)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotationComparisonDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotationComparisonDetail_QuotationComparisonHeader_QuotationConfirmedHeaderId",
                        column: x => x.QuotationConfirmedHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "QuotationComparisonHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuotationComparisonDetail_QuotationHeader_QuotationHeaderId",
                        column: x => x.QuotationHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "QuotationHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotationComparisonDetail_QuotationConfirmedHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                column: "QuotationConfirmedHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationComparisonDetail_QuotationHeaderId",
                schema: "Purchase",
                table: "QuotationComparisonDetail",
                column: "QuotationHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationComparisonHeader_RfqId",
                schema: "Purchase",
                table: "QuotationComparisonHeader",
                column: "RfqId");

            migrationBuilder.CreateIndex(
                name: "IX_QuotationComparisonHeader_StatusId",
                schema: "Purchase",
                table: "QuotationComparisonHeader",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotationComparisonDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "QuotationComparisonHeader",
                schema: "Purchase");

            migrationBuilder.CreateTable(
                name: "QuotationConfirmedHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfqId = table.Column<int>(type: "int", nullable: false),
                    ConfirmedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(400)", nullable: false),
                    RfqCode = table.Column<string>(type: "varchar(30)", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(400)", nullable: false)
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
                    QuotationDetailId = table.Column<int>(type: "int", nullable: false),
                    QuotationHeaderId = table.Column<int>(type: "int", nullable: false),
                    DeliveryDays = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    Freight = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    FreightPerItem = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    GstAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    GstPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0.00m),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(400)", nullable: false),
                    LandedUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Net = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(400)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Uom = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    ValidTill = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
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
    }
}
