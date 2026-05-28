using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseReturnRtvFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReturnType",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(30)", nullable: false),
                    Description = table.Column<string>(type: "varchar(100)", nullable: false),
                    InventoryImpactId = table.Column<int>(type: "int", nullable: true),
                    FinanceImpactId = table.Column<int>(type: "int", nullable: true),
                    IsReplacementApplicable = table.Column<bool>(type: "bit", nullable: false),
                    IsQcMandatory = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalRoleCode = table.Column<string>(type: "varchar(30)", nullable: true),
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
                    table.PrimaryKey("PK_ReturnType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnType_MiscMaster_FinanceImpactId",
                        column: x => x.FinanceImpactId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReturnType_MiscMaster_InventoryImpactId",
                        column: x => x.InventoryImpactId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnReason",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(30)", nullable: false),
                    Description = table.Column<string>(type: "varchar(150)", nullable: false),
                    ReturnTypeId = table.Column<int>(type: "int", nullable: false),
                    IsReplacementOverride = table.Column<bool>(type: "bit", nullable: true),
                    IsDebitNoteOverride = table.Column<bool>(type: "bit", nullable: true),
                    IsQcMandatoryOverride = table.Column<bool>(type: "bit", nullable: true),
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
                    table.PrimaryKey("PK_ReturnReason", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnReason_ReturnType_ReturnTypeId",
                        column: x => x.ReturnTypeId,
                        principalSchema: "Purchase",
                        principalTable: "ReturnType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RtvNumber = table.Column<string>(type: "varchar(40)", nullable: false),
                    RtvDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    PoId = table.Column<int>(type: "int", nullable: false),
                    GrnHeaderId = table.Column<int>(type: "int", nullable: false),
                    ReturnTypeId = table.Column<int>(type: "int", nullable: false),
                    ReturnReasonId = table.Column<int>(type: "int", nullable: false),
                    ReturnActionId = table.Column<int>(type: "int", nullable: false),
                    IsReplacementRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsDebitNoteRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsQcVerified = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ApprovalRequestId = table.Column<int>(type: "int", nullable: true),
                    ReplacementStatusId = table.Column<int>(type: "int", nullable: true),
                    ReplacementClosedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseReturnHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHeader_GrnHeader_GrnHeaderId",
                        column: x => x.GrnHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "GrnHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHeader_MiscMaster_ReplacementStatusId",
                        column: x => x.ReplacementStatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHeader_MiscMaster_ReturnActionId",
                        column: x => x.ReturnActionId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHeader_PurchaseOrderHeader_PoId",
                        column: x => x.PoId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHeader_ReturnReason_ReturnReasonId",
                        column: x => x.ReturnReasonId,
                        principalSchema: "Purchase",
                        principalTable: "ReturnReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnHeader_ReturnType_ReturnTypeId",
                        column: x => x.ReturnTypeId,
                        principalSchema: "Purchase",
                        principalTable: "ReturnType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseReturnHeaderId = table.Column<int>(type: "int", nullable: false),
                    GrnDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    AcceptedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    ReturnQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    RatePerUnit = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 6, nullable: true),
                    LineValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: true),
                    ReturnReasonId = table.Column<int>(type: "int", nullable: true),
                    LineRemarks = table.Column<string>(type: "varchar(300)", nullable: true),
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
                    table.PrimaryKey("PK_PurchaseReturnDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetail_GrnDetail_GrnDetailId",
                        column: x => x.GrnDetailId,
                        principalSchema: "Purchase",
                        principalTable: "GrnDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetail_PurchaseReturnHeader_PurchaseReturnHeaderId",
                        column: x => x.PurchaseReturnHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseReturnHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnDetail_ReturnReason_ReturnReasonId",
                        column: x => x.ReturnReasonId,
                        principalSchema: "Purchase",
                        principalTable: "ReturnReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetail_GrnDetailId",
                schema: "Purchase",
                table: "PurchaseReturnDetail",
                column: "GrnDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetail_ItemId",
                schema: "Purchase",
                table: "PurchaseReturnDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetail_PurchaseReturnHeaderId",
                schema: "Purchase",
                table: "PurchaseReturnDetail",
                column: "PurchaseReturnHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnDetail_ReturnReasonId",
                schema: "Purchase",
                table: "PurchaseReturnDetail",
                column: "ReturnReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHeader_GrnHeaderId",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                column: "GrnHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHeader_PoId",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                column: "PoId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHeader_ReplacementStatusId",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                column: "ReplacementStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHeader_ReturnActionId",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                column: "ReturnActionId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHeader_ReturnReasonId",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                column: "ReturnReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHeader_ReturnTypeId",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                column: "ReturnTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHeader_StatusId",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHeader_Unit_NotDeleted",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                columns: new[] { "UnitId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnHeader_VendorId",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "UQ_PurchaseReturnHeader_RtvNumber",
                schema: "Purchase",
                table: "PurchaseReturnHeader",
                column: "RtvNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnReason_ReturnTypeId",
                schema: "Purchase",
                table: "ReturnReason",
                column: "ReturnTypeId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReturnReason_Code",
                schema: "Purchase",
                table: "ReturnReason",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnType_FinanceImpactId",
                schema: "Purchase",
                table: "ReturnType",
                column: "FinanceImpactId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnType_InventoryImpactId",
                schema: "Purchase",
                table: "ReturnType",
                column: "InventoryImpactId");

            migrationBuilder.CreateIndex(
                name: "UQ_ReturnType_Code",
                schema: "Purchase",
                table: "ReturnType",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseReturnDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchaseReturnHeader",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "ReturnReason",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "ReturnType",
                schema: "Purchase");
        }
    }
}
