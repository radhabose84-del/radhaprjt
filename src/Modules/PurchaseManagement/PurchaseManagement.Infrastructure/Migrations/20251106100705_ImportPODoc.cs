using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImportPODoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseDocuments_MiscMaster_DocumentId",
                schema: "Purchase",
                table: "PurchaseDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDocuments_PoId",
                schema: "Purchase",
                table: "PurchaseDocuments",
                column: "PoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseDocuments_MiscMaster_DocumentId",
                schema: "Purchase",
                table: "PurchaseDocuments",
                column: "DocumentId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseDocuments_PurchaseOrderHeader_PoId",
                schema: "Purchase",
                table: "PurchaseDocuments",
                column: "PoId",
                principalSchema: "Purchase",
                principalTable: "PurchaseOrderHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseDocuments_MiscMaster_DocumentId",
                schema: "Purchase",
                table: "PurchaseDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseDocuments_PurchaseOrderHeader_PoId",
                schema: "Purchase",
                table: "PurchaseDocuments");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseDocuments_PoId",
                schema: "Purchase",
                table: "PurchaseDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseDocuments_MiscMaster_DocumentId",
                schema: "Purchase",
                table: "PurchaseDocuments",
                column: "DocumentId",
                principalSchema: "Purchase",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
