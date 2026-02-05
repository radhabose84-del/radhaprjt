using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StockTablesCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockLedger",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<string>(type: "varchar(50)", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocSlNo = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    ReceivedValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    IssueValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLedger", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubStoreStockLedger",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<string>(type: "varchar(50)", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocSlNo = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: true),
                    TargetId = table.Column<int>(type: "int", nullable: true),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    ReceivedValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    IssueValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubStoreStockLedger", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockLedger",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "SubStoreStockLedger",
                schema: "Inventory");
        }
    }
}
