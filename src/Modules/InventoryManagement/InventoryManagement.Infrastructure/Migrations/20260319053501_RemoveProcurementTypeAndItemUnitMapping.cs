using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProcurementTypeAndItemUnitMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemUnitMapping",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ProcurementType",
                schema: "Inventory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcurementType",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(255)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(255)", nullable: true),
                    ProcurementCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    ProcurementName = table.Column<string>(type: "varchar(100)", nullable: false)
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
                    ItemGroupId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ProcurementId = table.Column<int>(type: "int", nullable: false),
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
    }
}
