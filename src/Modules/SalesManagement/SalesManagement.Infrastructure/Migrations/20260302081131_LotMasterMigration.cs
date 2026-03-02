using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LotMasterMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LotMaster",
                schema: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LotCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    BatchNumber = table.Column<string>(type: "varchar(50)", nullable: false),
                    LotTypeId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ProductionOrderRef = table.Column<string>(type: "varchar(50)", nullable: true),
                    TotalProducedQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false, defaultValue: 0m),
                    AvailableQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 6, nullable: false, defaultValue: 0m),
                    RunOutDate = table.Column<DateOnly>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_LotMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LotMaster_MiscMaster_LotTypeId",
                        column: x => x.LotTypeId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LotMaster_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Sales",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LotMaster_ItemId",
                schema: "Sales",
                table: "LotMaster",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LotMaster_LotCode",
                schema: "Sales",
                table: "LotMaster",
                column: "LotCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_LotMaster_LotTypeId",
                schema: "Sales",
                table: "LotMaster",
                column: "LotTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LotMaster_StatusId",
                schema: "Sales",
                table: "LotMaster",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LotMaster",
                schema: "Sales");
        }
    }
}
