using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class usagetype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsageType",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsageTypeCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    UsageTypeName = table.Column<string>(type: "varchar(100)", nullable: false),
                    Description = table.Column<string>(type: "varchar(250)", nullable: true),
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
                    table.PrimaryKey("PK_UsageType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemUsageTypeMapping",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UsageTypeId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemUsageTypeMapping", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemUsageTypeMapping_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemUsageTypeMapping_UsageType_UsageTypeId",
                        column: x => x.UsageTypeId,
                        principalSchema: "Inventory",
                        principalTable: "UsageType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemUsageTypeMapping_ItemId_UsageTypeId_UnitId",
                schema: "Inventory",
                table: "ItemUsageTypeMapping",
                columns: new[] { "ItemId", "UsageTypeId", "UnitId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemUsageTypeMapping_UsageTypeId",
                schema: "Inventory",
                table: "ItemUsageTypeMapping",
                column: "UsageTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageType_UsageTypeCode",
                schema: "Inventory",
                table: "UsageType",
                column: "UsageTypeCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemUsageTypeMapping",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "UsageType",
                schema: "Inventory");
        }
    }
}
