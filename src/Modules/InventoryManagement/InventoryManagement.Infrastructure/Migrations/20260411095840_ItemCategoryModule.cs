using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemCategoryModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemCategoryModule",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemCategoryId = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemCategoryModule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemCategoryModule_ItemCategory_ItemCategoryId",
                        column: x => x.ItemCategoryId,
                        principalSchema: "Inventory",
                        principalTable: "ItemCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoryModule_ItemCategoryId",
                schema: "Inventory",
                table: "ItemCategoryModule",
                column: "ItemCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoryModule_ItemCategoryId_ModuleId",
                schema: "Inventory",
                table: "ItemCategoryModule",
                columns: new[] { "ItemCategoryId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemCategoryModule_ModuleId",
                schema: "Inventory",
                table: "ItemCategoryModule",
                column: "ModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemCategoryModule",
                schema: "Inventory");
        }
    }
}
