using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BarcodeSeriesMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "BarcodeSeries",
                schema: "Purchase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BarcodeSeriesNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    PrefixId = table.Column<int>(type: "int", nullable: false),
                    BarcodeStartNumber = table.Column<long>(type: "bigint", nullable: false),
                    BarcodeEndNumber = table.Column<long>(type: "bigint", nullable: false),
                    GenerationDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AllocatedCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_BarcodeSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BarcodeSeries_MiscMaster_PrefixId",
                        column: x => x.PrefixId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BarcodeSeries_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Purchase",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeSeries_BarcodeSeriesNumber",
                schema: "Purchase",
                table: "BarcodeSeries",
                column: "BarcodeSeriesNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeSeries_PrefixId",
                schema: "Purchase",
                table: "BarcodeSeries",
                column: "PrefixId");

            migrationBuilder.CreateIndex(
                name: "IX_BarcodeSeries_StatusId",
                schema: "Purchase",
                table: "BarcodeSeries",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BarcodeSeries",
                schema: "Purchase");

        }
    }
}
