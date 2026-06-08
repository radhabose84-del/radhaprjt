using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FreightRfq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FreightRfqHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FreightRfqNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    RfqDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RfqTypeId = table.Column<int>(type: "int", nullable: false),
                    PoReferenceId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    SourceLocation = table.Column<string>(type: "varchar(100)", nullable: false),
                    SourceStation = table.Column<string>(type: "varchar(100)", nullable: false),
                    DestinationLocation = table.Column<string>(type: "varchar(100)", nullable: false),
                    DestinationStation = table.Column<string>(type: "varchar(100)", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TotalBaleCount = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    SelectedQuotationId = table.Column<int>(type: "int", nullable: true),
                    ComparisonRemarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    ApprovedTransporterId = table.Column<int>(type: "int", nullable: true),
                    ApprovedRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: true),
                    ApprovedFreightValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: true),
                    ApprovalRemarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreightRfqHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FreightRfqHeader_PurchaseOrder",
                        column: x => x.PoReferenceId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FreightRfqHeader_RfqType",
                        column: x => x.RfqTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FreightRfqHeader_Status",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FreightRfqQuotation",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FreightRfqHeaderId = table.Column<int>(type: "int", nullable: false),
                    TransporterId = table.Column<int>(type: "int", nullable: false),
                    RateBasisId = table.Column<int>(type: "int", nullable: false),
                    QuotedRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    NoOfVehicles = table.Column<int>(type: "int", nullable: true),
                    FreightValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    IsOverride = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreightRfqQuotation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FreightRfqQuotation_FreightRfqHeader_FreightRfqHeaderId",
                        column: x => x.FreightRfqHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "FreightRfqHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FreightRfqQuotation_RateBasis",
                        column: x => x.RateBasisId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FreightRfqHeader_PoReferenceId",
                schema: "Purchase",
                table: "FreightRfqHeader",
                column: "PoReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FreightRfqHeader_RfqTypeId",
                schema: "Purchase",
                table: "FreightRfqHeader",
                column: "RfqTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FreightRfqHeader_StatusId",
                schema: "Purchase",
                table: "FreightRfqHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UQ_FreightRfqHeader_Number_NotDeleted",
                schema: "Purchase",
                table: "FreightRfqHeader",
                columns: new[] { "FreightRfqNumber", "IsDeleted" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_FreightRfqQuotation_HeaderId",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                column: "FreightRfqHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_FreightRfqQuotation_RateBasisId",
                schema: "Purchase",
                table: "FreightRfqQuotation",
                column: "RateBasisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FreightRfqQuotation",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "FreightRfqHeader",
                schema: "Purchase");
        }
    }
}
