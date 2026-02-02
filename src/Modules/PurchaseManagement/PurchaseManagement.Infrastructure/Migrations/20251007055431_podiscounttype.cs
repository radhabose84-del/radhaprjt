using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class podiscounttype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseLocalDetail_MiscMaster_DicountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseLocalDetail_DicountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail");

            migrationBuilder.DropColumn(
                name: "DicountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail");

            migrationBuilder.AddColumn<int>(
                name: "DiscountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseLocalDetail_DiscountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                column: "DiscountTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseLocalDetail_MiscMaster_DiscountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                column: "DiscountTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseLocalDetail_MiscMaster_DiscountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseLocalDetail_DiscountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail");

            migrationBuilder.DropColumn(
                name: "DiscountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail");

            migrationBuilder.AddColumn<int>(
                name: "DicountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseLocalDetail_DicountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                column: "DicountTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseLocalDetail_MiscMaster_DicountTypeId",
                schema: "Purchase",
                table: "PurchaseLocalDetail",
                column: "DicountTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
