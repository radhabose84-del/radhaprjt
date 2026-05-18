using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ContractPurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractPOHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ContractPONumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ContractDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ValidityFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValidityTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalContractValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UtilizedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractPOHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractPOHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ContractPODetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractPOHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    ContractQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ContractRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ContractValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UtilizedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalanceQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UtilizedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HSNId = table.Column<int>(type: "int", nullable: true),
                    GSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractPODetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractPODetail_ContractPOHeader_ContractPOHeaderId",
                        column: x => x.ContractPOHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "ContractPOHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseContractHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ContractPOHeaderId = table.Column<int>(type: "int", nullable: false),
                    IsPartialReceiptAllowed = table.Column<bool>(type: "bit", nullable: false),
                    IncotermsId = table.Column<int>(type: "int", nullable: true),
                    ModeOfDispatchId = table.Column<int>(type: "int", nullable: true),
                    FreightCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    TermsId = table.Column<int>(type: "int", nullable: true),
                    TermDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BillingAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseContractHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseContractHeader_ContractPOHeader_ContractPOHeaderId",
                        column: x => x.ContractPOHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "ContractPOHeader",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseContractHeader_MiscMaster_IncotermsId",
                        column: x => x.IncotermsId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseContractHeader_MiscMaster_ModeOfDispatchId",
                        column: x => x.ModeOfDispatchId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseContractHeader_PurchaseOrderHeader_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractPOReleaseHistory",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractPOHeaderId = table.Column<int>(type: "int", nullable: false),
                    ContractPODetailId = table.Column<int>(type: "int", nullable: false),
                    ReleasePOId = table.Column<int>(type: "int", nullable: false),
                    ReleaseDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReleasedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReleasedRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReleasedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractPOReleaseHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractPOReleaseHistory_ContractPODetail_ContractPODetailId",
                        column: x => x.ContractPODetailId,
                        principalSchema: "Purchase",
                        principalTable: "ContractPODetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContractPOReleaseHistory_ContractPOHeader_ContractPOHeaderId",
                        column: x => x.ContractPOHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "ContractPOHeader",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContractPOReleaseHistory_PurchaseOrderHeader_ReleasePOId",
                        column: x => x.ReleasePOId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseContractDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseContractHeaderId = table.Column<int>(type: "int", nullable: false),
                    ContractPODetailId = table.Column<int>(type: "int", nullable: false),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountTypeId = table.Column<int>(type: "int", nullable: true),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PandFType = table.Column<int>(type: "int", nullable: true),
                    PandFCharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    OtherCharge = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    GSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CGSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    SGSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IGSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CGST = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SGST = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IGST = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ScheduleDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DepartmentId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseContractDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseContractDetail_ContractPODetail_ContractPODetailId",
                        column: x => x.ContractPODetailId,
                        principalSchema: "Purchase",
                        principalTable: "ContractPODetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseContractDetail_MiscMaster_DiscountTypeId",
                        column: x => x.DiscountTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseContractDetail_PurchaseContractHeader_PurchaseContractHeaderId",
                        column: x => x.PurchaseContractHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseContractHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractPODetail_ContractPOHeaderId",
                schema: "Purchase",
                table: "ContractPODetail",
                column: "ContractPOHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPODetail_ItemId",
                schema: "Purchase",
                table: "ContractPODetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPOHeader_ContractPONumber",
                schema: "Purchase",
                table: "ContractPOHeader",
                column: "ContractPONumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractPOHeader_StatusId",
                schema: "Purchase",
                table: "ContractPOHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPOHeader_UnitId",
                schema: "Purchase",
                table: "ContractPOHeader",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPOHeader_VendorId",
                schema: "Purchase",
                table: "ContractPOHeader",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPOReleaseHistory_ContractPODetailId",
                schema: "Purchase",
                table: "ContractPOReleaseHistory",
                column: "ContractPODetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPOReleaseHistory_ContractPOHeaderId",
                schema: "Purchase",
                table: "ContractPOReleaseHistory",
                column: "ContractPOHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPOReleaseHistory_ReleasePOId",
                schema: "Purchase",
                table: "ContractPOReleaseHistory",
                column: "ReleasePOId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractDetail_ContractPODetailId",
                schema: "Purchase",
                table: "PurchaseContractDetail",
                column: "ContractPODetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractDetail_DiscountTypeId",
                schema: "Purchase",
                table: "PurchaseContractDetail",
                column: "DiscountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractDetail_PurchaseContractHeaderId",
                schema: "Purchase",
                table: "PurchaseContractDetail",
                column: "PurchaseContractHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractHeader_ContractPOHeaderId",
                schema: "Purchase",
                table: "PurchaseContractHeader",
                column: "ContractPOHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractHeader_IncotermsId",
                schema: "Purchase",
                table: "PurchaseContractHeader",
                column: "IncotermsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractHeader_ModeOfDispatchId",
                schema: "Purchase",
                table: "PurchaseContractHeader",
                column: "ModeOfDispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseContractHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "PurchaseContractHeader",
                column: "PurchaseOrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractPOReleaseHistory",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchaseContractDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "ContractPODetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchaseContractHeader",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "ContractPOHeader",
                schema: "Purchase");
        }
    }
}
