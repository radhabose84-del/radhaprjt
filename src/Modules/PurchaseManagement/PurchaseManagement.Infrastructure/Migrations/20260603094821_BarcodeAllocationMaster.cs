using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BarcodeAllocationMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BarcodeAllocation",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AllocationNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    AllocationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EmployeeNo = table.Column<string>(type: "varchar(50)", nullable: false),
                    EmployeeName = table.Column<string>(type: "varchar(150)", nullable: false),
                    BarcodeSeriesId = table.Column<int>(type: "int", nullable: false),
                    BarcodeFrom = table.Column<long>(type: "bigint", nullable: false),
                    BarcodeTo = table.Column<long>(type: "bigint", nullable: false),
                    UsedQuantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(250)", nullable: true),
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
                    table.PrimaryKey("PK_BarcodeAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarcodeAllocation_BarcodeSeries_BarcodeSeriesId",
                        column: x => x.BarcodeSeriesId,
                        principalSchema: "Purchase",
                        principalTable: "BarcodeSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BarcodeAllocation_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeAllocation_AllocationNumber",
                schema: "Purchase",
                table: "BarcodeAllocation",
                column: "AllocationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeAllocation_BarcodeSeriesId",
                schema: "Purchase",
                table: "BarcodeAllocation",
                column: "BarcodeSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeAllocation_StatusId",
                schema: "Purchase",
                table: "BarcodeAllocation",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarcodeAllocation",
                schema: "Purchase");
        }
    }
}
