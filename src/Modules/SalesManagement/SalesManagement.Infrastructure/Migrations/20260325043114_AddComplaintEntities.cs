using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComplaintHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    ComplaintDate = table.Column<DateTime>(type: "date", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerAddress = table.Column<string>(type: "varchar(500)", nullable: true),
                    CustomerPIN = table.Column<string>(type: "varchar(10)", nullable: true),
                    CustomerMobile = table.Column<string>(type: "varchar(15)", nullable: true),
                    CustomerEmail = table.Column<string>(type: "varchar(100)", nullable: true),
                    CustomerPAN = table.Column<string>(type: "varchar(10)", nullable: true),
                    CustomerGSTNo = table.Column<string>(type: "varchar(15)", nullable: true),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    TotalOS = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    Outstanding = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    BalanceCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    Delay = table.Column<string>(type: "varchar(50)", nullable: true),
                    Ledger = table.Column<string>(type: "varchar(100)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ComplaintHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintHeaderId = table.Column<int>(type: "int", nullable: false),
                    InvoiceHeaderId = table.Column<int>(type: "int", nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "date", nullable: false),
                    InvoiceTypeId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    NumberOfPacks = table.Column<int>(type: "int", nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ComplaintDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintDetail_ComplaintHeader_ComplaintHeaderId",
                        column: x => x.ComplaintHeaderId,
                        principalSchema: "Sales",
                        principalTable: "ComplaintHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintDetail_InvoiceHeader_InvoiceHeaderId",
                        column: x => x.InvoiceHeaderId,
                        principalSchema: "Sales",
                        principalTable: "InvoiceHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ComplaintDetail_MiscMaster_InvoiceTypeId",
                        column: x => x.InvoiceTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplaintDetailNature",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintDetailId = table.Column<int>(type: "int", nullable: false),
                    NatureOfComplaintId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintDetailNature", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplaintDetailNature_ComplaintDetail_ComplaintDetailId",
                        column: x => x.ComplaintDetailId,
                        principalSchema: "Sales",
                        principalTable: "ComplaintDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComplaintDetailNature_MiscMaster_NatureOfComplaintId",
                        column: x => x.NatureOfComplaintId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDetail_ComplaintHeaderId",
                schema: "Sales",
                table: "ComplaintDetail",
                column: "ComplaintHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDetail_InvoiceHeaderId",
                schema: "Sales",
                table: "ComplaintDetail",
                column: "InvoiceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDetail_InvoiceTypeId",
                schema: "Sales",
                table: "ComplaintDetail",
                column: "InvoiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDetail_ItemId",
                schema: "Sales",
                table: "ComplaintDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDetailNature_ComplaintDetailId",
                schema: "Sales",
                table: "ComplaintDetailNature",
                column: "ComplaintDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintDetailNature_NatureOfComplaintId",
                schema: "Sales",
                table: "ComplaintDetailNature",
                column: "NatureOfComplaintId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintHeader_ComplaintDate",
                schema: "Sales",
                table: "ComplaintHeader",
                column: "ComplaintDate");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintHeader_ComplaintNumber",
                schema: "Sales",
                table: "ComplaintHeader",
                column: "ComplaintNumber",
                unique: true,
                filter: "[ComplaintNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintHeader_CustomerId",
                schema: "Sales",
                table: "ComplaintHeader",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplaintHeader_StatusId",
                schema: "Sales",
                table: "ComplaintHeader",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComplaintDetailNature",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "ComplaintDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "ComplaintHeader",
                schema: "Sales");
        }
    }
}
