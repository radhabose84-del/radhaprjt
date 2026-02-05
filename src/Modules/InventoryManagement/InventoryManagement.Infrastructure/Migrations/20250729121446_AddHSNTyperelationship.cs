using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHSNTyperelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HSNMaster_TypeId",
                schema: "Inventory",
                table: "HSNMaster",
                column: "TypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_HSNMaster_MiscMaster_Type",
                schema: "Inventory",
                table: "HSNMaster",
                column: "TypeId",
                principalSchema: "Inventory",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HSNMaster_MiscMaster_Type",
                schema: "Inventory",
                table: "HSNMaster");

            migrationBuilder.DropIndex(
                name: "IX_HSNMaster_TypeId",
                schema: "Inventory",
                table: "HSNMaster");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                schema: "Inventory",
                table: "HSNMaster",
                newName: "Type");
        }
    }
}
