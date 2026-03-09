using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EInvoiceHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IrnNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    AckNo = table.Column<string>(type: "varchar(50)", nullable: true),
                    AckDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TCS = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    GstNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    Discount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Cess = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ReverseCharge = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SignInvoice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignQrCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EwbNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    EwbDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EwbValidTill = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_EInvoiceHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EInvoiceHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InvoiceType = table.Column<int>(type: "int", nullable: false),
                    DispatchAdviceId = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    AgentId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    TransportMode = table.Column<int>(type: "int", nullable: true),
                    VehicleNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    TransporterName = table.Column<string>(type: "varchar(100)", nullable: true),
                    LRNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    LRDate = table.Column<DateOnly>(type: "date", nullable: true),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalWeight = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TaxableValue = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Freight = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Insurance = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    HandlingCharge = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TCSPercentage = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TCS = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    InvoiceAmountBeforeTCS = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_InvoiceHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceHeader_DispatchAdviceHeader_DispatchAdviceId",
                        column: x => x.DispatchAdviceId,
                        principalSchema: "Sales",
                        principalTable: "DispatchAdviceHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceHeader_MiscMaster_InvoiceType",
                        column: x => x.InvoiceType,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InvoiceHeader_MiscMaster_TransportMode",
                        column: x => x.TransportMode,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EInvoiceDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EInvoiceHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(200)", nullable: true),
                    HsnNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    NoOfBags = table.Column<int>(type: "int", nullable: false),
                    Qty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    GstPercentage = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: true),
                    UOM = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EInvoiceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EInvoiceDetail_EInvoiceHeader_EInvoiceHeaderId",
                        column: x => x.EInvoiceHeaderId,
                        principalSchema: "Sales",
                        principalTable: "EInvoiceHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    HsnCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    GstPercentage = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    LotNo = table.Column<string>(type: "varchar(50)", nullable: true),
                    NoOfBags = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    RatePerKg = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CgstPercentage = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SgstPercentage = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IgstPercentage = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: true),
                    UOM = table.Column<string>(type: "varchar(20)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceDetail_InvoiceHeader_InvoiceHeaderId",
                        column: x => x.InvoiceHeaderId,
                        principalSchema: "Sales",
                        principalTable: "InvoiceHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceDetail_PackType_PackTypeId",
                        column: x => x.PackTypeId,
                        principalSchema: "Sales",
                        principalTable: "PackType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceDetail_EInvoiceHeaderId",
                schema: "Sales",
                table: "EInvoiceDetail",
                column: "EInvoiceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceDetail_ItemId",
                schema: "Sales",
                table: "EInvoiceDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceHeader_InvoiceNo",
                schema: "Sales",
                table: "EInvoiceHeader",
                column: "InvoiceNo");

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceHeader_IrnNumber",
                schema: "Sales",
                table: "EInvoiceHeader",
                column: "IrnNumber",
                unique: true,
                filter: "[IrnNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceHeader_PartyId",
                schema: "Sales",
                table: "EInvoiceHeader",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceHeader_StatusId",
                schema: "Sales",
                table: "EInvoiceHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceHeader_UnitId",
                schema: "Sales",
                table: "EInvoiceHeader",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_InvoiceHeaderId",
                schema: "Sales",
                table: "InvoiceDetail",
                column: "InvoiceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_ItemId",
                schema: "Sales",
                table: "InvoiceDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceDetail_PackTypeId",
                schema: "Sales",
                table: "InvoiceDetail",
                column: "PackTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_DispatchAdviceId",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "DispatchAdviceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_FinancialYearId",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_InvoiceDate",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "InvoiceDate");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_InvoiceNo",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "InvoiceNo",
                unique: true,
                filter: "[InvoiceNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_InvoiceType",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "InvoiceType");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_PartyId",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_TransportMode",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "TransportMode");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceHeader_UnitId",
                schema: "Sales",
                table: "InvoiceHeader",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EInvoiceDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "InvoiceDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "EInvoiceHeader",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "InvoiceHeader",
                schema: "Sales");
        }
    }
}
