using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EWaybillSeparation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EwbDate",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "EwbValidTill",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.RenameColumn(
                name: "EwbNo",
                schema: "Finance",
                table: "EInvoiceHeader",
                newName: "ErrorCode");

            migrationBuilder.AddColumn<string>(
                name: "DocType",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "varchar(5)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EWaybillCreated",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IrnStatus",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "varchar(20)",
                nullable: true,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfSupply",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "varchar(2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StateCess",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SupplyType",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CessAmount",
                schema: "Finance",
                table: "EInvoiceDetail",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CessRate",
                schema: "Finance",
                table: "EInvoiceDetail",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FreeQty",
                schema: "Finance",
                table: "EInvoiceDetail",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossAmount",
                schema: "Finance",
                table: "EInvoiceDetail",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "IsService",
                schema: "Finance",
                table: "EInvoiceDetail",
                type: "char(1)",
                nullable: true,
                defaultValue: "N");

            migrationBuilder.CreateTable(
                name: "EWaybillHeader",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EInvoiceHeaderId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    EWBNumber = table.Column<string>(type: "varchar(20)", nullable: true),
                    InvoiceNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: true),
                    InvoiceValue = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SupplyType = table.Column<string>(type: "varchar(20)", nullable: true),
                    SubSupplyType = table.Column<string>(type: "varchar(30)", nullable: true),
                    DocumentType = table.Column<string>(type: "varchar(20)", nullable: true),
                    TransactionType = table.Column<int>(type: "int", nullable: true),
                    FromGSTIN = table.Column<string>(type: "varchar(15)", nullable: true),
                    FromTradeName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ToGSTIN = table.Column<string>(type: "varchar(15)", nullable: true),
                    ToTradeName = table.Column<string>(type: "varchar(100)", nullable: true),
                    TotalValue = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Cess = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    TransporterId = table.Column<int>(type: "int", nullable: true),
                    TransporterGSTIN = table.Column<string>(type: "varchar(15)", nullable: true),
                    TransporterName = table.Column<string>(type: "varchar(200)", nullable: true),
                    TransportMode = table.Column<string>(type: "varchar(5)", nullable: true),
                    TransDocNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    TransDocDate = table.Column<DateOnly>(type: "date", nullable: true),
                    VehicleNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    VehicleType = table.Column<string>(type: "varchar(5)", nullable: true),
                    Distance = table.Column<int>(type: "int", nullable: true),
                    PartyId = table.Column<int>(type: "int", nullable: true),
                    GeneratedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ValidUpto = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EwbStatus = table.Column<string>(type: "varchar(20)", nullable: true, defaultValue: "Pending"),
                    ErrorCode = table.Column<string>(type: "varchar(20)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(500)", nullable: true),
                    CancelledDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CancelReason = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_EWaybillHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWaybillHeader_EInvoiceHeader_EInvoiceHeaderId",
                        column: x => x.EInvoiceHeaderId,
                        principalSchema: "Finance",
                        principalTable: "EInvoiceHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EWaybillDetail",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EWaybillHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(200)", nullable: true),
                    HsnNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    IsService = table.Column<string>(type: "char(1)", nullable: true, defaultValue: "N"),
                    Qty = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    UOM = table.Column<string>(type: "varchar(20)", nullable: true),
                    TaxableValue = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IGST = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Cess = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EWaybillDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EWaybillDetail_EWaybillHeader_EWaybillHeaderId",
                        column: x => x.EWaybillHeaderId,
                        principalSchema: "Finance",
                        principalTable: "EWaybillHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EInvoiceHeader_IrnStatus",
                schema: "Finance",
                table: "EInvoiceHeader",
                column: "IrnStatus");

            migrationBuilder.CreateIndex(
                name: "IX_EWaybillDetail_EWaybillHeaderId",
                schema: "Finance",
                table: "EWaybillDetail",
                column: "EWaybillHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_EWaybillDetail_ItemId",
                schema: "Finance",
                table: "EWaybillDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EWaybillHeader_EInvoiceHeaderId",
                schema: "Finance",
                table: "EWaybillHeader",
                column: "EInvoiceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_EWaybillHeader_EWBNumber",
                schema: "Finance",
                table: "EWaybillHeader",
                column: "EWBNumber",
                unique: true,
                filter: "[EWBNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EWaybillHeader_EwbStatus",
                schema: "Finance",
                table: "EWaybillHeader",
                column: "EwbStatus");

            migrationBuilder.CreateIndex(
                name: "IX_EWaybillHeader_PartyId",
                schema: "Finance",
                table: "EWaybillHeader",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_EWaybillHeader_TransporterId",
                schema: "Finance",
                table: "EWaybillHeader",
                column: "TransporterId");

            migrationBuilder.CreateIndex(
                name: "IX_EWaybillHeader_UnitId",
                schema: "Finance",
                table: "EWaybillHeader",
                column: "UnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EWaybillDetail",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "EWaybillHeader",
                schema: "Finance");

            migrationBuilder.DropIndex(
                name: "IX_EInvoiceHeader_IrnStatus",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "DocType",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "EWaybillCreated",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "IrnStatus",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "PlaceOfSupply",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "Remarks",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "StateCess",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "SupplyType",
                schema: "Finance",
                table: "EInvoiceHeader");

            migrationBuilder.DropColumn(
                name: "CessAmount",
                schema: "Finance",
                table: "EInvoiceDetail");

            migrationBuilder.DropColumn(
                name: "CessRate",
                schema: "Finance",
                table: "EInvoiceDetail");

            migrationBuilder.DropColumn(
                name: "FreeQty",
                schema: "Finance",
                table: "EInvoiceDetail");

            migrationBuilder.DropColumn(
                name: "GrossAmount",
                schema: "Finance",
                table: "EInvoiceDetail");

            migrationBuilder.DropColumn(
                name: "IsService",
                schema: "Finance",
                table: "EInvoiceDetail");

            migrationBuilder.RenameColumn(
                name: "ErrorCode",
                schema: "Finance",
                table: "EInvoiceHeader",
                newName: "EwbNo");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EwbDate",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EwbValidTill",
                schema: "Finance",
                table: "EInvoiceHeader",
                type: "datetimeoffset",
                nullable: true);
        }
    }
}
