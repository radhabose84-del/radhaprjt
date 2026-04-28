using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesQuotationAmendment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RevisionNumber",
                schema: "Sales",
                table: "SalesQuotationHeader",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SalesQuotationAmendmentHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesQuotationHeaderId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AmendmentNo = table.Column<string>(type: "varchar(50)", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    AmendmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    TotalBasicAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    TotalDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    NetTaxableAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    TotalTax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_SalesQuotationAmendmentHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesQuotationAmendmentHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesQuotationAmendmentHeader_SalesQuotationHeader_SalesQuotationHeaderId",
                        column: x => x.SalesQuotationHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesQuotationHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesQuotationAmendmentDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesQuotationAmendmentHeaderId = table.Column<int>(type: "int", nullable: false),
                    ChangeType = table.Column<string>(type: "varchar(10)", nullable: false),
                    SalesQuotationDetailId = table.Column<int>(type: "int", nullable: false),
                    OldItemId = table.Column<int>(type: "int", nullable: false),
                    OldQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    OldExMillRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: false),
                    OldDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    OldHSNId = table.Column<int>(type: "int", nullable: false),
                    OldTaxPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    NewItemId = table.Column<int>(type: "int", nullable: true),
                    NewQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: true),
                    NewExMillRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: true),
                    NewDiscount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: true),
                    NewHSNId = table.Column<int>(type: "int", nullable: true),
                    NewTaxPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: true),
                    NetRate = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesQuotationAmendmentDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesQuotationAmendmentDetail_SalesQuotationAmendmentHeader_SalesQuotationAmendmentHeaderId",
                        column: x => x.SalesQuotationAmendmentHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesQuotationAmendmentHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationAmendmentDetail_SalesQuotationAmendmentHeaderId",
                schema: "Sales",
                table: "SalesQuotationAmendmentDetail",
                column: "SalesQuotationAmendmentHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationAmendmentDetail_SalesQuotationDetailId",
                schema: "Sales",
                table: "SalesQuotationAmendmentDetail",
                column: "SalesQuotationDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationAmendmentHeader_AmendmentNo",
                schema: "Sales",
                table: "SalesQuotationAmendmentHeader",
                column: "AmendmentNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationAmendmentHeader_SalesQuotationHeaderId",
                schema: "Sales",
                table: "SalesQuotationAmendmentHeader",
                column: "SalesQuotationHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationAmendmentHeader_StatusId",
                schema: "Sales",
                table: "SalesQuotationAmendmentHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesQuotationAmendmentHeader_UnitId",
                schema: "Sales",
                table: "SalesQuotationAmendmentHeader",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesQuotationAmendmentDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesQuotationAmendmentHeader",
                schema: "Sales");

            migrationBuilder.DropColumn(
                name: "RevisionNumber",
                schema: "Sales",
                table: "SalesQuotationHeader");
        }
    }
}
