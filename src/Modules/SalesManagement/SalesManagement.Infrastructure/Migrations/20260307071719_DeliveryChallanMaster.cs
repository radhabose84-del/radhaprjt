using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryChallanMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryChallanHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryNumber = table.Column<string>(type: "varchar(30)", nullable: false),
                    DeliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StoHeaderId = table.Column<int>(type: "int", nullable: false),
                    FromPlantId = table.Column<int>(type: "int", nullable: false),
                    FromStorageLocationId = table.Column<int>(type: "int", nullable: false),
                    ToPlantId = table.Column<int>(type: "int", nullable: false),
                    ToStorageLocationId = table.Column<int>(type: "int", nullable: false),
                    TransporterId = table.Column<int>(type: "int", nullable: false),
                    VehicleNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    TransportDistance = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: true),
                    DeliveryValue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    ConsignmentValue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
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
                    table.PrimaryKey("PK_DeliveryChallanHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryChallanHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryChallanHeader_StoHeader_StoHeaderId",
                        column: x => x.StoHeaderId,
                        principalSchema: "Sales",
                        principalTable: "StoHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryChallanDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryChallanHeaderId = table.Column<int>(type: "int", nullable: false),
                    StoDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    DispatchQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    UOMId = table.Column<int>(type: "int", nullable: false),
                    BagCount = table.Column<int>(type: "int", nullable: true),
                    BaleCount = table.Column<int>(type: "int", nullable: true),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    GrossWeight = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: true),
                    ExMillRate = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    LineMovementValue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryChallanDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryChallanDetail_DeliveryChallanHeader_DeliveryChallanHeaderId",
                        column: x => x.DeliveryChallanHeaderId,
                        principalSchema: "Sales",
                        principalTable: "DeliveryChallanHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryChallanDetail_LotMaster_LotId",
                        column: x => x.LotId,
                        principalSchema: "Sales",
                        principalTable: "LotMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeliveryChallanDetail_StoDetail_StoDetailId",
                        column: x => x.StoDetailId,
                        principalSchema: "Sales",
                        principalTable: "StoDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanDetail_DeliveryChallanHeaderId",
                schema: "Sales",
                table: "DeliveryChallanDetail",
                column: "DeliveryChallanHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanDetail_ItemId",
                schema: "Sales",
                table: "DeliveryChallanDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanDetail_LotId",
                schema: "Sales",
                table: "DeliveryChallanDetail",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanDetail_StoDetailId",
                schema: "Sales",
                table: "DeliveryChallanDetail",
                column: "StoDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_DeliveryDate",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "DeliveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_DeliveryNumber",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "DeliveryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_FromPlantId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "FromPlantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_StatusId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_StoHeaderId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "StoHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_ToPlantId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "ToPlantId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanHeader_TransporterId",
                schema: "Sales",
                table: "DeliveryChallanHeader",
                column: "TransporterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryChallanDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DeliveryChallanHeader",
                schema: "Sales");
        }
    }
}
