using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class itemmasteronspot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnSpot",
                schema: "Inventory",
                table: "ItemMaster",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IssueRuleId",
                schema: "Inventory",
                table: "ItemMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaster_IssueRuleId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "IssueRuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItemMaster_MiscMaster_IssueRuleId",
                schema: "Inventory",
                table: "ItemMaster",
                column: "IssueRuleId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItemMaster_MiscMaster_IssueRuleId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropIndex(
                name: "IX_ItemMaster_IssueRuleId",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropColumn(
                name: "IsOnSpot",
                schema: "Inventory",
                table: "ItemMaster");

            migrationBuilder.DropColumn(
                name: "IssueRuleId",
                schema: "Inventory",
                table: "ItemMaster");
        }
    }
}
