using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OCREntryMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalFlag",
                schema: "Purchase",
                table: "TnCTemplateMaster");

            migrationBuilder.CreateTable(
                name: "OCREntry",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OcrNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    OcrDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcurementSourceId = table.Column<int>(type: "int", nullable: false),
                    ProcurementTypeId = table.Column<int>(type: "int", nullable: false),
                    BrokerDirectId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    PaymentTermId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    StationId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    CountId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Rate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedDispatchDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DocumentPath = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(20)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OCREntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OCREntry_MiscMaster_BrokerDirectId",
                        column: x => x.BrokerDirectId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OCREntry_MiscMaster_GradeId",
                        column: x => x.GradeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OCREntry_MiscMaster_ProcurementSourceId",
                        column: x => x.ProcurementSourceId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OCREntry_MiscMaster_ProcurementTypeId",
                        column: x => x.ProcurementTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OCREntry_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OCREntry_PaymentTermMaster_PaymentTermId",
                        column: x => x.PaymentTermId,
                        principalSchema: "Purchase",
                        principalTable: "PaymentTermMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_BrokerDirectId",
                schema: "Purchase",
                table: "OCREntry",
                column: "BrokerDirectId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_GradeId",
                schema: "Purchase",
                table: "OCREntry",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_OcrDate",
                schema: "Purchase",
                table: "OCREntry",
                column: "OcrDate");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_OcrNumber",
                schema: "Purchase",
                table: "OCREntry",
                column: "OcrNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_PaymentTermId",
                schema: "Purchase",
                table: "OCREntry",
                column: "PaymentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_ProcurementSourceId",
                schema: "Purchase",
                table: "OCREntry",
                column: "ProcurementSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_ProcurementTypeId",
                schema: "Purchase",
                table: "OCREntry",
                column: "ProcurementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_StatusId",
                schema: "Purchase",
                table: "OCREntry",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OCREntry_SupplierId",
                schema: "Purchase",
                table: "OCREntry",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OCREntry",
                schema: "Purchase");

            migrationBuilder.AddColumn<bool>(
                name: "ApprovalFlag",
                schema: "Purchase",
                table: "TnCTemplateMaster",
                type: "bit",
                nullable: true);
        }
    }
}
