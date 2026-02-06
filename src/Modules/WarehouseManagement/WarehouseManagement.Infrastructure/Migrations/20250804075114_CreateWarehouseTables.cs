using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WarehouseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateWarehouseTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Warehouse");

            migrationBuilder.CreateTable(
                name: "WarehouseMaster",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseCode = table.Column<string>(type: "varchar(50)", nullable: false),
                    WarehouseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ParentWarehouseId = table.Column<int>(type: "int", nullable: true),
                    IsGroup = table.Column<bool>(type: "bit", nullable: false),
                    WarehouseTypeId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    CapacityUOMId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: true),
                    ContactPersonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    Pincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsScrapWarehouse = table.Column<bool>(type: "bit", nullable: false),
                    IsTransitWarehouse = table.Column<bool>(type: "bit", nullable: false),
                    MaxCapacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDefaultStockEntry = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseMaster_WarehouseMaster_ParentWarehouseId",
                        column: x => x.ParentWarehouseId,
                        principalSchema: "Warehouse",
                        principalTable: "WarehouseMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseItemGroupMapping",
                schema: "Warehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ItemGroupId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseItemGroupMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseItemGroupMapping_WarehouseMaster_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "Warehouse",
                        principalTable: "WarehouseMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseItemGroupMapping_WarehouseId_ItemGroupId",
                schema: "Warehouse",
                table: "WarehouseItemGroupMapping",
                columns: new[] { "WarehouseId", "ItemGroupId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseMaster_ParentWarehouseId",
                schema: "Warehouse",
                table: "WarehouseMaster",
                column: "ParentWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseMaster_WarehouseCode",
                schema: "Warehouse",
                table: "WarehouseMaster",
                column: "WarehouseCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarehouseItemGroupMapping",
                schema: "Warehouse");

            migrationBuilder.DropTable(
                name: "WarehouseMaster",
                schema: "Warehouse");
        }
    }
}
