using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DispatchAdviceMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DispatchAdviceHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispatchNo = table.Column<string>(type: "varchar(30)", nullable: false),
                    DispatchDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderId = table.Column<int>(type: "int", nullable: false),
                    PartyId = table.Column<int>(type: "int", nullable: false),
                    TotOrderQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TotDispatchedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    TotPendingQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false),
                    DispatchAddressId = table.Column<int>(type: "int", nullable: false),
                    TransporterId = table.Column<int>(type: "int", nullable: true),
                    VehicleNo = table.Column<string>(type: "varchar(20)", nullable: true),
                    DriverName = table.Column<string>(type: "varchar(100)", nullable: true),
                    LRNo = table.Column<string>(type: "varchar(50)", nullable: true),
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
                    table.PrimaryKey("PK_DispatchAdviceHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchAdviceHeader_DispatchAddressMaster_DispatchAddressId",
                        column: x => x.DispatchAddressId,
                        principalSchema: "Sales",
                        principalTable: "DispatchAddressMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispatchAdviceHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispatchAdviceHeader_SalesOrderHeader_SalesOrderId",
                        column: x => x.SalesOrderId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DispatchAdviceDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DispatchAdviceHeaderId = table.Column<int>(type: "int", nullable: false),
                    SalesOrderDetailId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    StartPackNo = table.Column<int>(type: "int", nullable: false),
                    EndPackNo = table.Column<int>(type: "int", nullable: false),
                    DispatchQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchAdviceDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchAdviceDetail_DispatchAdviceHeader_DispatchAdviceHeaderId",
                        column: x => x.DispatchAdviceHeaderId,
                        principalSchema: "Sales",
                        principalTable: "DispatchAdviceHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispatchAdviceDetail_LotMaster_LotId",
                        column: x => x.LotId,
                        principalSchema: "Sales",
                        principalTable: "LotMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispatchAdviceDetail_SalesOrderDetail_SalesOrderDetailId",
                        column: x => x.SalesOrderDetailId,
                        principalSchema: "Sales",
                        principalTable: "SalesOrderDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceDetail_DispatchAdviceHeaderId",
                schema: "Sales",
                table: "DispatchAdviceDetail",
                column: "DispatchAdviceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceDetail_ItemId",
                schema: "Sales",
                table: "DispatchAdviceDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceDetail_LotId",
                schema: "Sales",
                table: "DispatchAdviceDetail",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceDetail_SalesOrderDetailId",
                schema: "Sales",
                table: "DispatchAdviceDetail",
                column: "SalesOrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_DispatchAddressId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "DispatchAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_DispatchDate",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "DispatchDate");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_DispatchNo",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "DispatchNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_PartyId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_SalesOrderId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "SalesOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchAdviceHeader_StatusId",
                schema: "Sales",
                table: "DispatchAdviceHeader",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispatchAdviceDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "DispatchAdviceHeader",
                schema: "Sales");
        }
    }
}
