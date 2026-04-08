using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductionStockLedger_AddStockClosing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LooseConeLedger",
                schema: "Production");

            migrationBuilder.DropColumn(
                name: "AsonLooseKgs",
                schema: "Production",
                table: "RepackingHeader");

            migrationBuilder.DropColumn(
                name: "LooseConeIn",
                schema: "Production",
                table: "RepackingHeader");

            migrationBuilder.DropColumn(
                name: "AsonLooseKgs",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "LooseConeIn",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "StockClosing",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.RenameColumn(
                name: "LooseConeOut",
                schema: "Production",
                table: "RepackingHeader",
                newName: "LooseConeKgs");

            migrationBuilder.RenameColumn(
                name: "LooseConeOut",
                schema: "Production",
                table: "ProductionPackDetail",
                newName: "LooseConeKgs");

            migrationBuilder.CreateTable(
                name: "ProductionStockLedger",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    DocTypeId = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateTime>(type: "date", nullable: false),
                    OpeningLooseKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ProdKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalProdKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    PackTypeId = table.Column<int>(type: "int", nullable: false),
                    NetWeightPerPack = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    NetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    BagsRepacked = table.Column<int>(type: "int", nullable: false),
                    RepackKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ClosingLooseKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ClosingPackKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ClosingBags = table.Column<int>(type: "int", nullable: false),
                    StockClosing = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionStockLedger", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStockLedger_DocTypeId",
                schema: "Production",
                table: "ProductionStockLedger",
                column: "DocTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionStockLedger_UnitId_ItemId_LotId_DocDate_Id",
                schema: "Production",
                table: "ProductionStockLedger",
                columns: new[] { "UnitId", "ItemId", "LotId", "DocDate", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionStockLedger",
                schema: "Production");

            migrationBuilder.RenameColumn(
                name: "LooseConeKgs",
                schema: "Production",
                table: "RepackingHeader",
                newName: "LooseConeOut");

            migrationBuilder.RenameColumn(
                name: "LooseConeKgs",
                schema: "Production",
                table: "ProductionPackDetail",
                newName: "LooseConeOut");

            migrationBuilder.AddColumn<decimal>(
                name: "AsonLooseKgs",
                schema: "Production",
                table: "RepackingHeader",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LooseConeIn",
                schema: "Production",
                table: "RepackingHeader",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AsonLooseKgs",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LooseConeIn",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "StockClosing",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LooseConeLedger",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AsonLooseKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    DocDate = table.Column<DateTime>(type: "date", nullable: false),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocTypeId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LooseConeIn = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    LooseConeOut = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LooseConeLedger", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LooseConeLedger_UnitId_ItemId_LotId_DocDate_Id",
                schema: "Production",
                table: "LooseConeLedger",
                columns: new[] { "UnitId", "ItemId", "LotId", "DocDate", "Id" });
        }
    }
}
