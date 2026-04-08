using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductionPackDetailFlatStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackDetail_ProductionPackHeader_ProductionPackHeaderId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropTable(
                name: "ProductionPackHeader",
                schema: "Production");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_BinId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_ProductionPackHeaderId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "LineRemarks",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.RenameColumn(
                name: "ProductionPackHeaderId",
                schema: "Production",
                table: "ProductionPackDetail",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "ItemSno",
                schema: "Production",
                table: "ProductionPackDetail",
                newName: "UnitId");

            migrationBuilder.AlterColumn<int>(
                name: "LotId",
                schema: "Production",
                table: "RepackingHeader",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StartPackNo",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "QualityStatusId",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EndPackNo",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BinId",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedIP",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LooseConeKgs",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LotMasterId",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PackDate",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PackNo",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "varchar(30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductionKgs",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ProductionYear",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "nvarchar(500)",
                nullable: true);

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
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    LotId = table.Column<int>(type: "int", nullable: false),
                    DocType = table.Column<string>(type: "varchar(10)", nullable: true),
                    DocNo = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateTime>(type: "date", nullable: false),
                    LooseConeIn = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    LooseConeOut = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    AsonLooseKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LooseConeLedger", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_ItemId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_LotMasterId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "LotMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_MiscMasterId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_PackDate",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_PackNo_ProductionYear",
                schema: "Production",
                table: "ProductionPackDetail",
                columns: new[] { "PackNo", "ProductionYear" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackTypeId1");

            migrationBuilder.CreateIndex(
                name: "IX_LooseConeLedger_UnitId_ItemId_LotId_DocDate_Id",
                schema: "Production",
                table: "LooseConeLedger",
                columns: new[] { "UnitId", "ItemId", "LotId", "DocDate", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackDetail_LotMaster_LotMasterId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "LotMasterId",
                principalSchema: "Production",
                principalTable: "LotMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackDetail_MiscMaster_MiscMasterId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "MiscMasterId",
                principalSchema: "Production",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackDetail_PackType_PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "PackTypeId1",
                principalSchema: "Production",
                principalTable: "PackType",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackDetail_LotMaster_LotMasterId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackDetail_MiscMaster_MiscMasterId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionPackDetail_PackType_PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropTable(
                name: "LooseConeLedger",
                schema: "Production");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_ItemId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_LotMasterId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_MiscMasterId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_PackDate",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_PackNo_ProductionYear",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropIndex(
                name: "IX_ProductionPackDetail_PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "CreatedIP",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "LooseConeKgs",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "LotMasterId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "PackDate",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "PackNo",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "PackTypeId1",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "ProductionKgs",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "ProductionYear",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "Remarks",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.DropColumn(
                name: "StockClosing",
                schema: "Production",
                table: "ProductionPackDetail");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                schema: "Production",
                table: "ProductionPackDetail",
                newName: "ProductionPackHeaderId");

            migrationBuilder.RenameColumn(
                name: "UnitId",
                schema: "Production",
                table: "ProductionPackDetail",
                newName: "ItemSno");

            migrationBuilder.AlterColumn<int>(
                name: "LotId",
                schema: "Production",
                table: "RepackingHeader",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "StartPackNo",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "QualityStatusId",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EndPackNo",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BinId",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineRemarks",
                schema: "Production",
                table: "ProductionPackDetail",
                type: "nvarchar(250)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductionPackHeader",
                schema: "Production",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LooseConeKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    PackDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PackNo = table.Column<string>(type: "varchar(30)", nullable: false),
                    ProductionKgs = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ProductionYear = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", nullable: true),
                    TotalBags = table.Column<int>(type: "int", nullable: false),
                    TotalNetWeight = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionPackHeader", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_BinId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "BinId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackDetail_ProductionPackHeaderId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "ProductionPackHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_PackDate",
                schema: "Production",
                table: "ProductionPackHeader",
                column: "PackDate");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_PackNo_ProductionYear",
                schema: "Production",
                table: "ProductionPackHeader",
                columns: new[] { "PackNo", "ProductionYear" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionPackHeader_WarehouseId",
                schema: "Production",
                table: "ProductionPackHeader",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionPackDetail_ProductionPackHeader_ProductionPackHeaderId",
                schema: "Production",
                table: "ProductionPackDetail",
                column: "ProductionPackHeaderId",
                principalSchema: "Production",
                principalTable: "ProductionPackHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
