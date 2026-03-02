using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesQuotationMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesQuotationHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    QuotationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SalesEnquiryId = table.Column<int>(type: "int", nullable: true),
                    ContactPersonId = table.Column<int>(type: "int", nullable: true),
                    ValidityDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    DeliveryTermId = table.Column<int>(type: "int", nullable: false),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalDiscount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    NetTaxableAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalTax = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_SalesQuotationHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesQuotationHeader_MiscMaster_DeliveryTermId",
                        column: x => x.DeliveryTermId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesQuotationHeader_SalesContact_ContactPersonId",
                        column: x => x.ContactPersonId,
                        principalSchema: "Sales",
                        principalTable: "SalesContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesQuotationHeader_SalesEnquiryHeader_SalesEnquiryId",
                        column: x => x.SalesEnquiryId,
                        principalSchema: "Sales",
                        principalTable: "SalesEnquiryHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotationDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesQuotationHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ExMillRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    NetRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    HSNId = table.Column<int>(type: "int", nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesQuotationDetail_SalesQuotationHeader_SalesQuotationHeaderId",
                        column: x => x.SalesQuotationHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesQuotationHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDetail_HSNId",
                schema: "Sales",
                table: "SalesQuotationDetail",
                column: "HSNId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDetail_ItemId",
                schema: "Sales",
                table: "SalesQuotationDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationDetail_SalesQuotationHeaderId",
                schema: "Sales",
                table: "SalesQuotationDetail",
                column: "SalesQuotationHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHeader_ContactPersonId",
                schema: "Sales",
                table: "SalesQuotationHeader",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHeader_CustomerId",
                schema: "Sales",
                table: "SalesQuotationHeader",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHeader_DeliveryTermId",
                schema: "Sales",
                table: "SalesQuotationHeader",
                column: "DeliveryTermId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHeader_PaymentTermId",
                schema: "Sales",
                table: "SalesQuotationHeader",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationHeader_SalesEnquiryId",
                schema: "Sales",
                table: "SalesQuotationHeader",
                column: "SalesEnquiryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesQuotationDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesQuotationHeader",
                schema: "Sales");
        }
    }
}
