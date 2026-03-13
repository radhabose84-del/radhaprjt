using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFinanceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tables moved to FinanceManagement module — no DB changes.
            // Finance.TransactionTypeMaster and Finance.DocumentSequence remain in the database,
            // now owned by FinanceManagement.Infrastructure.
            // Sales.EInvoiceHeader and Sales.EInvoiceDetail were never migrated to DB from Sales context.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Finance");

            migrationBuilder.CreateTable(
                name: "EInvoiceHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusId = table.Column<int>(type: "int", nullable: true),
                    AckDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AckNo = table.Column<string>(type: "varchar(50)", nullable: true),
                    CGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Cess = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    Discount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    EwbDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EwbNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    EwbValidTill = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    GstNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    IGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    InvoiceAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    InvoiceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    InvoiceNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    IrnNumber = table.Column<string>(type: "varchar(100)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    ReverseCharge = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RoundOff = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SignInvoice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignQrCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TCS = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
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
                name: "TransactionTypeMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    Description = table.Column<string>(type: "varchar(255)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModuleId = table.Column<int>(type: "int", nullable: false),
                    ShortName = table.Column<string>(type: "varchar(50)", nullable: false),
                    TypeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypeMaster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EInvoiceDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EInvoiceHeaderId = table.Column<int>(type: "int", nullable: false),
                    CGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    GstPercentage = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    HsnNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    IGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "varchar(200)", nullable: true),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    NoOfBags = table.Column<int>(type: "int", nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: true),
                    Qty = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    SGST = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TaxableAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    UOM = table.Column<string>(type: "varchar(20)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false)
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
                name: "DocumentSequence",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentSequence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentSequence_TransactionTypeMaster_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalSchema: "Finance",
                        principalTable: "TransactionTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSequence_FinancialYearId",
                schema: "Finance",
                table: "DocumentSequence",
                column: "FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSequence_TransactionTypeId",
                schema: "Finance",
                table: "DocumentSequence",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSequence_TransactionTypeId_FinancialYearId_DocNo",
                schema: "Finance",
                table: "DocumentSequence",
                columns: new[] { "TransactionTypeId", "FinancialYearId", "DocNo" },
                unique: true);

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
                name: "IX_TransactionTypeMaster_MenuId",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_ModuleId",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_ShortName",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "ShortName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_TypeName",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "TypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypeMaster_UnitId",
                schema: "Finance",
                table: "TransactionTypeMaster",
                column: "UnitId");
        }
    }
}
