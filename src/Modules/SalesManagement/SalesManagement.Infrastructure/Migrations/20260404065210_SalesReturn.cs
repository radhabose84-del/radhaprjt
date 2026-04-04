using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SalesReturn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesReturnHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReturnNumber = table.Column<string>(type: "varchar(30)", nullable: false),
                    ReturnDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ComplaintHeaderId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    BinId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_SalesReturnHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturnHeader_ComplaintHeader_ComplaintHeaderId",
                        column: x => x.ComplaintHeaderId,
                        principalSchema: "Sales",
                        principalTable: "ComplaintHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturnHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesReturnHeaderId = table.Column<int>(type: "int", nullable: false),
                    InvoiceHeaderId = table.Column<int>(type: "int", nullable: false),
                    InvoiceDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: true),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    ReturnQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: true),
                    BagStatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SalesReturnDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetail_InvoiceHeader_InvoiceHeaderId",
                        column: x => x.InvoiceHeaderId,
                        principalSchema: "Sales",
                        principalTable: "InvoiceHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetail_MiscMaster_BagStatusId",
                        column: x => x.BagStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SalesReturnDetail_SalesReturnHeader_SalesReturnHeaderId",
                        column: x => x.SalesReturnHeaderId,
                        principalSchema: "Sales",
                        principalTable: "SalesReturnHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetail_BagStatusId",
                schema: "Sales",
                table: "SalesReturnDetail",
                column: "BagStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetail_InvoiceDetailId",
                schema: "Sales",
                table: "SalesReturnDetail",
                column: "InvoiceDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetail_InvoiceHeaderId",
                schema: "Sales",
                table: "SalesReturnDetail",
                column: "InvoiceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnDetail_SalesReturnHeaderId",
                schema: "Sales",
                table: "SalesReturnDetail",
                column: "SalesReturnHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnHeader_ComplaintHeaderId",
                schema: "Sales",
                table: "SalesReturnHeader",
                column: "ComplaintHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnHeader_CustomerId",
                schema: "Sales",
                table: "SalesReturnHeader",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnHeader_ReturnNumber",
                schema: "Sales",
                table: "SalesReturnHeader",
                column: "ReturnNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnHeader_StatusId",
                schema: "Sales",
                table: "SalesReturnHeader",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesReturnDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "SalesReturnHeader",
                schema: "Sales");
        }
    }
}
