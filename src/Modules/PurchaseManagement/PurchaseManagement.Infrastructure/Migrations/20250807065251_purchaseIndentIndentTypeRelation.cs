using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class purchaseIndentIndentTypeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_IndentHeader_IndentTypeId",
                schema: "Purchase",
                table: "IndentHeader",
                column: "IndentTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_IndentHeader_MiscMaster_IndentTypeId",
                schema: "Purchase",
                table: "IndentHeader",
                column: "IndentTypeId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IndentHeader_MiscMaster_IndentTypeId",
                schema: "Purchase",
                table: "IndentHeader");

            migrationBuilder.DropIndex(
                name: "IX_IndentHeader_IndentTypeId",
                schema: "Purchase",
                table: "IndentHeader");
        }
    }
}
