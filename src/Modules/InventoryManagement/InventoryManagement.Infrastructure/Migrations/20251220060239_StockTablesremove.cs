using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StockTablesremove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockLedger",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "SubStoreStockLedger",
                schema: "Inventory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockLedger",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocSlNo = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<string>(type: "varchar(50)", nullable: false),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    IssueValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    ReceivedValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
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
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocSlNo = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<string>(type: "varchar(50)", nullable: false),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    IssueValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    ReceivedValue = table.Column<decimal>(type: "decimal(18,3)", nullable: true, defaultValue: 0.00m),
                    StorageTypeId = table.Column<int>(type: "int", nullable: true),
                    TargetId = table.Column<int>(type: "int", nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    UomId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubStoreStockLedger", x => x.Id);
                });
        }
    }
}
