using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BlanketPOMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlanketHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    BlanketNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BlanketDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ProcurementTypeId = table.Column<int>(type: "int", nullable: false),
                    BrokerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValidityFrom = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ValidityTo = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeliveryTerms = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    TotalEstimatedValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_BlanketHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlanketHeader_MiscMaster_ProcurementTypeId",
                        column: x => x.ProcurementTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BlanketHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BlanketDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlanketHeaderId = table.Column<int>(type: "int", nullable: false),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    EstimatedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HSNId = table.Column<int>(type: "int", nullable: true),
                    GSTPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    QualitySpecification = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BlanketDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlanketDetail_BlanketHeader_BlanketHeaderId",
                        column: x => x.BlanketHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "BlanketHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseBlanketHeader",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    BlanketHeaderId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PurchaseBlanketHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseBlanketHeader_BlanketHeader_BlanketHeaderId",
                        column: x => x.BlanketHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "BlanketHeader",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseBlanketHeader_MiscMaster_IncotermsId",
                        column: x => x.IncotermsId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseBlanketHeader_MiscMaster_ModeOfDispatchId",
                        column: x => x.ModeOfDispatchId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseBlanketHeader_PurchaseOrderHeader_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlanketSchedule",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BlanketDetailId = table.Column<int>(type: "int", nullable: false),
                    ScheduleNo = table.Column<int>(type: "int", nullable: false),
                    ScheduleDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ScheduleQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
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
                    table.PrimaryKey("PK_BlanketSchedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlanketSchedule_BlanketDetail_BlanketDetailId",
                        column: x => x.BlanketDetailId,
                        principalSchema: "Purchase",
                        principalTable: "BlanketDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseBlanketDetail",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseBlanketHeaderId = table.Column<int>(type: "int", nullable: false),
                    BlanketDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemSno = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
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
                    table.PrimaryKey("PK_PurchaseBlanketDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseBlanketDetail_BlanketDetail_BlanketDetailId",
                        column: x => x.BlanketDetailId,
                        principalSchema: "Purchase",
                        principalTable: "BlanketDetail",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseBlanketDetail_MiscMaster_DiscountTypeId",
                        column: x => x.DiscountTypeId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PurchaseBlanketDetail_PurchaseBlanketHeader_PurchaseBlanketHeaderId",
                        column: x => x.PurchaseBlanketHeaderId,
                        principalSchema: "Purchase",
                        principalTable: "PurchaseBlanketHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlanketDetail_BlanketHeaderId",
                schema: "Purchase",
                table: "BlanketDetail",
                column: "BlanketHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BlanketDetail_ItemId",
                schema: "Purchase",
                table: "BlanketDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BlanketHeader_BlanketNumber",
                schema: "Purchase",
                table: "BlanketHeader",
                column: "BlanketNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlanketHeader_ProcurementTypeId",
                schema: "Purchase",
                table: "BlanketHeader",
                column: "ProcurementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BlanketHeader_StatusId",
                schema: "Purchase",
                table: "BlanketHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_BlanketHeader_UnitId",
                schema: "Purchase",
                table: "BlanketHeader",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BlanketHeader_VendorId",
                schema: "Purchase",
                table: "BlanketHeader",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_BlanketSchedule_BlanketDetailId",
                schema: "Purchase",
                table: "BlanketSchedule",
                column: "BlanketDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseBlanketDetail_BlanketDetailId",
                schema: "Purchase",
                table: "PurchaseBlanketDetail",
                column: "BlanketDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseBlanketDetail_DiscountTypeId",
                schema: "Purchase",
                table: "PurchaseBlanketDetail",
                column: "DiscountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseBlanketDetail_PurchaseBlanketHeaderId",
                schema: "Purchase",
                table: "PurchaseBlanketDetail",
                column: "PurchaseBlanketHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseBlanketHeader_BlanketHeaderId",
                schema: "Purchase",
                table: "PurchaseBlanketHeader",
                column: "BlanketHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseBlanketHeader_IncotermsId",
                schema: "Purchase",
                table: "PurchaseBlanketHeader",
                column: "IncotermsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseBlanketHeader_ModeOfDispatchId",
                schema: "Purchase",
                table: "PurchaseBlanketHeader",
                column: "ModeOfDispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseBlanketHeader_PurchaseOrderId",
                schema: "Purchase",
                table: "PurchaseBlanketHeader",
                column: "PurchaseOrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlanketSchedule",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchaseBlanketDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "BlanketDetail",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "PurchaseBlanketHeader",
                schema: "Purchase");

            migrationBuilder.DropTable(
                name: "BlanketHeader",
                schema: "Purchase");
        }
    }
}
