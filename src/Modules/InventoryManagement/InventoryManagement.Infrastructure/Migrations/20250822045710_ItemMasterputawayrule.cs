using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ItemMasterputawayrule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PutAwayRule_ItemCategory_ItemCategoryId1",
                schema: "Inventory",
                table: "PutAwayRule");

            migrationBuilder.DropForeignKey(
                name: "FK_PutAwayRule_ItemGroup_ItemGroupId1",
                schema: "Inventory",
                table: "PutAwayRule");

            migrationBuilder.DropForeignKey(
                name: "FK_PutAwayStrategy_MiscMaster_MiscMasterId",
                schema: "Inventory",
                table: "PutAwayStrategy");

            migrationBuilder.DropForeignKey(
                name: "FK_PutAwayStrategy_MiscMaster_MiscMasterId1",
                schema: "Inventory",
                table: "PutAwayStrategy");

            migrationBuilder.DropIndex(
                name: "IX_PutAwayStrategy_MiscMasterId",
                schema: "Inventory",
                table: "PutAwayStrategy");

            migrationBuilder.DropIndex(
                name: "IX_PutAwayStrategy_MiscMasterId1",
                schema: "Inventory",
                table: "PutAwayStrategy");

            migrationBuilder.DropIndex(
                name: "IX_PutAwayRule_ItemCategoryId1",
                schema: "Inventory",
                table: "PutAwayRule");

            migrationBuilder.DropIndex(
                name: "IX_PutAwayRule_ItemGroupId1",
                schema: "Inventory",
                table: "PutAwayRule");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "Inventory",
                table: "PutAwayStrategy");

            migrationBuilder.DropColumn(
                name: "MiscMasterId1",
                schema: "Inventory",
                table: "PutAwayStrategy");

            migrationBuilder.DropColumn(
                name: "ItemCategoryId1",
                schema: "Inventory",
                table: "PutAwayRule");

            migrationBuilder.DropColumn(
                name: "ItemGroupId1",
                schema: "Inventory",
                table: "PutAwayRule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "Inventory",
                table: "PutAwayStrategy",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId1",
                schema: "Inventory",
                table: "PutAwayStrategy",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemCategoryId1",
                schema: "Inventory",
                table: "PutAwayRule",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemGroupId1",
                schema: "Inventory",
                table: "PutAwayRule",
                type: "int",
                nullable: true);

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
                name: "IX_PutAwayRule_ItemCategoryId1",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemCategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_PutAwayRule_ItemGroupId1",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemGroupId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PutAwayRule_ItemCategory_ItemCategoryId1",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemCategoryId1",
                principalSchema: "Inventory",
                principalTable: "ItemCategory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PutAwayRule_ItemGroup_ItemGroupId1",
                schema: "Inventory",
                table: "PutAwayRule",
                column: "ItemGroupId1",
                principalSchema: "Inventory",
                principalTable: "ItemGroup",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PutAwayStrategy_MiscMaster_MiscMasterId",
                schema: "Inventory",
                table: "PutAwayStrategy",
                column: "MiscMasterId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PutAwayStrategy_MiscMaster_MiscMasterId1",
                schema: "Inventory",
                table: "PutAwayStrategy",
                column: "MiscMasterId1",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
