using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class issuereturnIdchangetblmigartion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssueReturnDetail_IssueReturnHeader_IssueHeaderId",
                schema: "Purchase",
                table: "IssueReturnDetail");

            migrationBuilder.RenameColumn(
                name: "IssueHeaderId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                newName: "IssueReturnHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_IssueReturnDetail_IssueHeaderId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                newName: "IX_IssueReturnDetail_IssueReturnHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueReturnDetail_IssueReturnHeader_IssueReturnHeaderId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                column: "IssueReturnHeaderId",
                principalSchema: "Purchase",
                principalTable: "IssueReturnHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IssueReturnDetail_IssueReturnHeader_IssueReturnHeaderId",
                schema: "Purchase",
                table: "IssueReturnDetail");

            migrationBuilder.RenameColumn(
                name: "IssueReturnHeaderId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                newName: "IssueHeaderId");

            migrationBuilder.RenameIndex(
                name: "IX_IssueReturnDetail_IssueReturnHeaderId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                newName: "IX_IssueReturnDetail_IssueHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_IssueReturnDetail_IssueReturnHeader_IssueHeaderId",
                schema: "Purchase",
                table: "IssueReturnDetail",
                column: "IssueHeaderId",
                principalSchema: "Purchase",
                principalTable: "IssueReturnHeader",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
