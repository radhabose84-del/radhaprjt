using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMaster_Putawayrule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PutAwayRule",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    ItemGroupId = table.Column<int>(type: "int", nullable: false),
                    ItemCategoryId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: true),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    ItemCategoryId1 = table.Column<int>(type: "int", nullable: true),
                    ItemGroupId1 = table.Column<int>(type: "int", nullable: true),
                    ItemMasterId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PutAwayRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PutAwayRule_ItemCategory_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalSchema: "Inventory",
                        principalTable: "ItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PutAwayRule_ItemCategory_ItemCategoryId1",
                        column: x => x.ItemCategoryId1,
                        principalSchema: "Inventory",
                        principalTable: "ItemCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PutAwayRule_ItemGroup_ItemGroupId",
                        column: x => x.ItemGroupId,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PutAwayRule_ItemGroup_ItemGroupId1",
                        column: x => x.ItemGroupId1,
                        principalSchema: "Inventory",
                        principalTable: "ItemGroup",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PutAwayRule_ItemMaster_ItemId",
                        column: x => x.ItemId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PutAwayRule_ItemMaster_ItemMasterId",
                        column: x => x.ItemMasterId,
                        principalSchema: "Inventory",
                        principalTable: "ItemMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PutAwayStrategy",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PutAwayRuleId = table.Column<int>(type: "int", nullable: false),
                    StorageTypeId = table.Column<int>(type: "int", nullable: false),
                    TargetId = table.Column<int>(type: "int", nullable: true),
                    PriorityId = table.Column<int>(type: "int", nullable: false),
                    MiscMasterId = table.Column<int>(type: "int", nullable: true),
                    MiscMasterId1 = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PutAwayStrategy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PutAwayStrategy_MiscMaster_MiscMasterId",
                        column: x => x.MiscMasterId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PutAwayStrategy_MiscMaster_MiscMasterId1",
                        column: x => x.MiscMasterId1,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PutAwayStrategy_MiscMaster_PriorityId",
                        column: x => x.PriorityId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PutAwayStrategy_MiscMaster_StorageTypeId",
                        column: x => x.StorageTypeId,
                        principalSchema: "Inventory",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PutAwayStrategy_PutAwayRule_PutAwayRuleId",
                        column: x => x.PutAwayRuleId,
                        principalSchema: "Inventory",
                        principalTable: "PutAwayRule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayRule_ItemCategoryId",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayRule_ItemCategoryId1",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemCategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayRule_ItemGroupId",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayRule_ItemGroupId1",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemGroupId1");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayRule_ItemId",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayRule_ItemMasterId",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemMasterId");

            migrationBuilder.CreateIndex(
                name: "UX_PutAwayRule_Scope",
                schema: "Inventory",
                table: "PutAwayRule",
                columns: new[] { "UnitId", "WarehouseId", "ItemGroupId", "ItemCategoryId", "ItemId" },
                unique: true,
                filter: "[ItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayStrategy_MiscMasterId",
                schema: "Inventory",
                table: "PutAwayStrategy",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayStrategy_MiscMasterId1",
                schema: "Inventory",
                table: "PutAwayStrategy",
                column: "MiscMasterId1");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayStrategy_PriorityId",
                schema: "Inventory",
                table: "PutAwayStrategy",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayStrategy_StorageTypeId",
                schema: "Inventory",
                table: "PutAwayStrategy",
                column: "StorageTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_PutAwayStrategy_PriorityPerRule",
                schema: "Inventory",
                table: "PutAwayStrategy",
                columns: new[] { "PutAwayRuleId", "PriorityId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PutAwayStrategy",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "PutAwayRule",
                schema: "Inventory");
        }
    }
}
