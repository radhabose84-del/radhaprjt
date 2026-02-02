using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseorderLocal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseOrderHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    PONumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    PODate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    POCategoryId = table.Column<int>(type: "int", nullable: false),
                    POMethodId = table.Column<int>(type: "int", nullable: true),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    ItemTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PandFTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MiscCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GSTTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CGSTTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SGSTTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IGSTTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    FreightTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    InsuranceTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TDSTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PurchaseValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PurchaseOrderHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrderHeader_MiscMaster_POCategoryId",
                        column: x => x.POCategoryId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseOrderHeader_MiscMaster_POMethodId",
                        column: x => x.POMethodId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseLocalHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: true),
                    ProjectId = table.Column<int>(type: "int", nullable: true),
                    IsPartialReceiptAllowed = table.Column<bool>(type: "bit", nullable: false),
                    IncotermsId = table.Column<int>(type: "int", nullable: true),
                    ModeOfDispatchId = table.Column<int>(type: "int", nullable: true),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TermsId = table.Column<int>(type: "int", nullable: true),
                    TermDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BillingAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    POImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseLocalHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseLocalHeader_MiscMaster_IncotermsId",
                        column: x => x.IncotermsId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseLocalHeader_MiscMaster_ModeOfDispatchId",
                        column: x => x.ModeOfDispatchId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseLocalHeader_PurchaseOrderHeader_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchasePaymentTerm",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: false),
                    AdvancePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreditDays = table.Column<int>(type: "int", nullable: true),
                    PaymentModelId = table.Column<int>(type: "int", nullable: true),
                    InsuranceId = table.Column<int>(type: "int", nullable: true),
                    InsurancePercent = table.Column<int>(type: "int", nullable: true),
                    InsuranceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    AdvanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BalancePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
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
                    table.PrimaryKey("PK_PurchasePaymentTerm", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchasePaymentTerm_MiscMaster_PaymentModelId",
                        column: x => x.PaymentModelId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchasePaymentTerm_MiscMaster_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchasePaymentTerm_PurchaseOrderHeader_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseLocalDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseLocalId = table.Column<int>(type: "int", nullable: false),
                    IndentId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    LastPOPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    DicountTypeId = table.Column<int>(type: "int", nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PandFType = table.Column<int>(type: "int", nullable: true),
                    PandFCharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OtherCharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    GSTPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CGST = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SGST = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IGST = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ScheduleDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    ItemValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_PurchaseLocalDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseLocalDetail_MiscMaster_DicountTypeId",
                        column: x => x.DicountTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseLocalDetail_PurchaseLocalHeader_PurchaseLocalId",
                        column: x => x.PurchaseLocalId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseLocalHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseLocalDetail_DicountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                column: "DicountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseLocalDetail_PurchaseLocalId",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                column: "PurchaseLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseLocalHeader_IncotermsId",
                schema: "Purchase",
                table: "PurchaseLocalHeader",
                column: "IncotermsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseLocalHeader_ModeOfDispatchId",
                schema: "Purchase",
                table: "PurchaseLocalHeader",
                column: "ModeOfDispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseLocalHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "PurchaseLocalHeader",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHeader_POCategoryId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                column: "POCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHeader_POMethodId",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                column: "POMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderHeader_PONumber",
                schema: "Purchase",
                table: "PurchaseOrderHeader",
                column: "PONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePaymentTerm_PaymentModelId",
                schema: "Purchase",
                table: "PurchasePaymentTerm",
                column: "PaymentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePaymentTerm_PaymentTermId",
                schema: "Purchase",
                table: "PurchasePaymentTerm",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchasePaymentTerm_PurchaseOrderId",
                schema: "Purchase",
                table: "PurchasePaymentTerm",
                column: "PurchaseOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseLocalDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchasePaymentTerm",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchaseLocalHeader",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchaseOrderHeader",
                schema: "Purchase");
        }
    }
}
