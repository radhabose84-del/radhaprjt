using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesOrderMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesOrderHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderNo = table.Column<string>(type: "varchar(30)", nullable: false),
                    OrderDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SalesGroupId = table.Column<int>(type: "int", nullable: false),
                    SalesSegmentId = table.Column<int>(type: "int", nullable: true),
                    EnquiryType = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    DiscountPlanId = table.Column<int>(type: "int", nullable: true),
                    PaymentTermsId = table.Column<int>(type: "int", nullable: false),
                    PaymentTypeId = table.Column<int>(type: "int", nullable: true),
                    FreightTypeId = table.Column<int>(type: "int", nullable: false),
                    CountListId = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    VisitNotesAttachment = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    AgentPOAttachment = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    DispatchLocationType = table.Column<int>(type: "int", nullable: false),
                    DispatchDepotId = table.Column<int>(type: "int", nullable: true),
                    DispatchUnitId = table.Column<int>(type: "int", nullable: true),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalWeightKgs = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TotalDiscountPerKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    ItemValue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TotalFreight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    GSTPercentage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TotalGST = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TotalWithGST = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TCSPercentage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TotalTCS = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    FinalAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_SalesOrderHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderHeader_MiscMaster_CountListId",
                        column: x => x.CountListId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderHeader_MiscMaster_DiscountPlanId",
                        column: x => x.DiscountPlanId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderHeader_MiscMaster_FreightTypeId",
                        column: x => x.FreightTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderHeader_MiscMaster_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderHeader_SalesGroup_SalesGroupId",
                        column: x => x.SalesGroupId,
                        principalSchema: "Sales",
                        principalTable: "SalesGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderHeader_SalesSegment_SalesSegmentId",
                        column: x => x.SalesSegmentId,
                        principalSchema: "Sales",
                        principalTable: "SalesSegment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesOrderDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesOrderHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    HSNId = table.Column<int>(type: "int", nullable: false),
                    QtyInBags = table.Column<int>(type: "int", nullable: false),
                    BagWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    SaleUOMId = table.Column<int>(type: "int", nullable: false),
                    TotalWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    ExMillRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    DiscountPerUnit = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    Freight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TCSPercentage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TCSAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    NetRatePerKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    ExpectedDeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AgentCommissionPercentage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    DispatchedQty = table.Column<int>(type: "int", nullable: false),
                    PendingQty = table.Column<int>(type: "int", nullable: false),
                    LineItemStatusId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesOrderDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetail_MiscMaster_LineItemStatusId",
                        column: x => x.LineItemStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesOrderDetail_SalesOrderHeader_SalesOrderHeaderId",
                        column: x => x.SalesOrderHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetail_HSNId",
                schema: "Sales",
                table: "SalesOrderDetail",
                column: "HSNId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetail_ItemId",
                schema: "Sales",
                table: "SalesOrderDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetail_LineItemStatusId",
                schema: "Sales",
                table: "SalesOrderDetail",
                column: "LineItemStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderDetail_SalesOrderHeaderId",
                schema: "Sales",
                table: "SalesOrderDetail",
                column: "SalesOrderHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_CountListId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "CountListId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_DiscountPlanId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "DiscountPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_FreightTypeId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "FreightTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_OrderDate",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_PartyId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_PaymentTypeId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_SalesGroupId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_SalesOrderNo",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesOrderNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesOrderHeader_SalesSegmentId",
                schema: "Sales",
                table: "SalesOrderHeader",
                column: "SalesSegmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesOrderDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesOrderHeader",
                schema: "Sales");
        }
    }
}
