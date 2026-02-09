using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBinMasterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BinMaster",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    RackId = table.Column<int>(type: "int", nullable: true),
                    BinCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BinName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BinCapacity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CapacityUOMId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_BinMaster", x => x.Id);
                    table.CheckConstraint("CK_BinCapacity_Positive", "[BinCapacity] > 0");
                    table.ForeignKey(
                        name: "FK_BinMaster_RackMaster_RackId",
                        column: x => x.RackId,
                        principalSchema: "Warehouse",
                        principalTable: "RackMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BinMaster_WarehouseMaster_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "Warehouse",
                        principalTable: "WarehouseMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BinMaster_RackId",
                schema: "Warehouse",
                table: "BinMaster",
                column: "RackId");

            migrationBuilder.CreateIndex(
                name: "UQ_Bin_Warehouse_BinCode",
                schema: "Warehouse",
                table: "BinMaster",
                columns: new[] { "WarehouseId", "BinCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BinMaster",
                schema: "Warehouse");
        }
    }
}
