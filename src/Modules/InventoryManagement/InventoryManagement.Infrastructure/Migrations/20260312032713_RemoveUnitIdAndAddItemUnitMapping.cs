using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnitIdAndAddItemUnitMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.CreateTable(
                name: "ProcurementType",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProcurementCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    ProcurementName = table.Column<string>(type: "varchar(100)", nullable: false),
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
                    table.PrimaryKey("PK_ProcurementType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemUnitMapping",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ProcurementId = table.Column<int>(type: "int", nullable: false),
                    ItemGroupId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemUnitMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemUnitMapping_ItemGroup_ItemGroupId",
                        column: x => x.ItemGroupId,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemUnitMapping_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemUnitMapping_ProcurementType_ProcurementId",
                        column: x => x.ProcurementId,
                        principalSchema: "Inventory",
                        principalTable: "ProcurementType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemUnitMapping_ItemGroupId",
                schema: "Inventory",
                table: "ItemUnitMapping",
                column: "ItemGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemUnitMapping_ItemId_ProcurementId_ItemGroupId_UnitId",
                schema: "Inventory",
                table: "ItemUnitMapping",
                columns: new[] { "ItemId", "ProcurementId", "ItemGroupId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemUnitMapping_ProcurementId",
                schema: "Inventory",
                table: "ItemUnitMapping",
                column: "ProcurementId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementType_ProcurementCode",
                schema: "Inventory",
                table: "ProcurementType",
                column: "ProcurementCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemUnitMapping",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ProcurementType",
                schema: "Inventory");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "Inventory",
                table: "ItemMaster",
                type: "int",
                nullable: true);
        }
    }
}
