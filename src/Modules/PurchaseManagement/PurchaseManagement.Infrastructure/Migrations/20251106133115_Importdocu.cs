using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Importdocu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseDocuments_PoId",
                schema: "Purchase",
                table: "PurchaseDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDocuments_PoId_DocumentId",
                schema: "Purchase",
                table: "PurchaseDocuments",
                columns: new[] { "PoId", "DocumentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PurchaseDocuments_PoId_DocumentId",
                schema: "Purchase",
                table: "PurchaseDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseDocuments_PoId",
                schema: "Purchase",
                table: "PurchaseDocuments",
                column: "PoId");
        }
    }
}
