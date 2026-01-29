using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class storesledgertablemigartion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockLedger",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OldUnitCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    TransactionType = table.Column<string>(type: "varchar(50)", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocSNo = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    UOM = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    ReceivedQty = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.00m),
                    ReceivedValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.00m),
                    IssueQty = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.00m),
                    IssueValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false, defaultValue: 0.00m),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLedger", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockLedger",
                schema: "Maintenance");
        }
    }
}
