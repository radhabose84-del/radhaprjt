using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemTransactionstablemigartion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemTransactions",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OldUnitCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    TC = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocSNo = table.Column<int>(type: "int", nullable: false),
                    DocDt = table.Column<DateTime>(type: "datetime", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    UOM = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    CatDesc = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    GrpName = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    LifeType = table.Column<string>(type: "varchar(10)", nullable: true),
                    LifeSpan = table.Column<int>(type: "int", nullable: false),
                    DepName = table.Column<string>(type: "nvarchar(200)", nullable: true),
                    CreatedDt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTransactions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemTransactions",
                schema: "Maintenance");
        }
    }
}
