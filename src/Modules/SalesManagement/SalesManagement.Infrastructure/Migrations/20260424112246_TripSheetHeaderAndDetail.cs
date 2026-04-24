using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class TripSheetHeaderAndDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TripSheetHeader",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripSheetNo = table.Column<string>(type: "varchar(30)", nullable: false),
                    TripDate = table.Column<DateOnly>(type: "date", nullable: false),
                    VehicleNo = table.Column<string>(type: "varchar(20)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_TripSheetHeader", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TripSheetDetail",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripSheetHeaderId = table.Column<int>(type: "int", nullable: false),
                    DispatchAdviceHeaderId = table.Column<int>(type: "int", nullable: false),
                    SequenceNo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripSheetDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripSheetDetail_DispatchAdviceHeader_DispatchAdviceHeaderId",
                        column: x => x.DispatchAdviceHeaderId,
                        principalSchema: "Sales",
                        principalTable: "DispatchAdviceHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripSheetDetail_TripSheetHeader_TripSheetHeaderId",
                        column: x => x.TripSheetHeaderId,
                        principalSchema: "Sales",
                        principalTable: "TripSheetHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TripSheetDetail_DispatchAdviceHeaderId",
                schema: "Sales",
                table: "TripSheetDetail",
                column: "DispatchAdviceHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_TripSheetDetail_TripSheetHeaderId",
                schema: "Sales",
                table: "TripSheetDetail",
                column: "TripSheetHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_TripSheetDetail_TripSheetHeaderId_DispatchAdviceHeaderId",
                schema: "Sales",
                table: "TripSheetDetail",
                columns: new[] { "TripSheetHeaderId", "DispatchAdviceHeaderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripSheetDetail_TripSheetHeaderId_SequenceNo",
                schema: "Sales",
                table: "TripSheetDetail",
                columns: new[] { "TripSheetHeaderId", "SequenceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripSheetHeader_TripDate",
                schema: "Sales",
                table: "TripSheetHeader",
                column: "TripDate");

            migrationBuilder.CreateIndex(
                name: "IX_TripSheetHeader_TripSheetNo",
                schema: "Sales",
                table: "TripSheetHeader",
                column: "TripSheetNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripSheetHeader_UnitId",
                schema: "Sales",
                table: "TripSheetHeader",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TripSheetHeader_VehicleNo",
                schema: "Sales",
                table: "TripSheetHeader",
                column: "VehicleNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TripSheetDetail",
                schema: "Sales");

            migrationBuilder.DropTable(
                name: "TripSheetHeader",
                schema: "Sales");
        }
    }
}
