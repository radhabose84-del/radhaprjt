using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoReceiptMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoReceiptHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoReceiptNumber = table.Column<string>(type: "varchar(30)", nullable: false),
                    StoReceiptDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DeliveryChallanHeaderId = table.Column<int>(type: "int", nullable: false),
                    ReceivingPlantId = table.Column<int>(type: "int", nullable: false),
                    ReceivingStorageLocationId = table.Column<int>(type: "int", nullable: false),
                    VehicleNumber = table.Column<string>(type: "varchar(50)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_StoReceiptHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoReceiptHeader_DeliveryChallanHeader_DeliveryChallanHeaderId",
                        column: x => x.DeliveryChallanHeaderId,
                        principalSchema: "Sales",
                        principalTable: "DeliveryChallanHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoReceiptHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StoReceiptDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoReceiptHeaderId = table.Column<int>(type: "int", nullable: false),
                    DeliveryChallanDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    DispatchQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    DamageQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    BagCount = table.Column<int>(type: "int", nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: true),
                    LineStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoReceiptDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoReceiptDetail_DeliveryChallanDetail_DeliveryChallanDetailId",
                        column: x => x.DeliveryChallanDetailId,
                        principalSchema: "Sales",
                        principalTable: "DeliveryChallanDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoReceiptDetail_LotMaster_LotId",
                        column: x => x.LotId,
                        principalSchema: "Sales",
                        principalTable: "LotMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoReceiptDetail_MiscMaster_LineStatusId",
                        column: x => x.LineStatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StoReceiptDetail_StoReceiptHeader_StoReceiptHeaderId",
                        column: x => x.StoReceiptHeaderId,
                        principalSchema: "Sales",
                        principalTable: "StoReceiptHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptDetail_DeliveryChallanDetailId",
                schema: "Sales",
                table: "StoReceiptDetail",
                column: "DeliveryChallanDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptDetail_ItemId",
                schema: "Sales",
                table: "StoReceiptDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptDetail_LineStatusId",
                schema: "Sales",
                table: "StoReceiptDetail",
                column: "LineStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptDetail_LotId",
                schema: "Sales",
                table: "StoReceiptDetail",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptDetail_StoReceiptHeaderId",
                schema: "Sales",
                table: "StoReceiptDetail",
                column: "StoReceiptHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptHeader_DeliveryChallanHeaderId",
                schema: "Sales",
                table: "StoReceiptHeader",
                column: "DeliveryChallanHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptHeader_ReceivingPlantId",
                schema: "Sales",
                table: "StoReceiptHeader",
                column: "ReceivingPlantId");

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptHeader_StatusId",
                schema: "Sales",
                table: "StoReceiptHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptHeader_StoReceiptDate",
                schema: "Sales",
                table: "StoReceiptHeader",
                column: "StoReceiptDate");

            migrationBuilder.CreateIndex(
                name: "IX_StoReceiptHeader_StoReceiptNumber",
                schema: "Sales",
                table: "StoReceiptHeader",
                column: "StoReceiptNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoReceiptDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "StoReceiptHeader",
                schema: "Sales");
        }
    }
}
