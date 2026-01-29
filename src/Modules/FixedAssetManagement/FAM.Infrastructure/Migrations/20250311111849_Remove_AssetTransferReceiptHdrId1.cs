using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Remove_AssetTransferReceiptHdrId1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
                // Drop the foreign key constraint
                migrationBuilder.DropForeignKey(
                    name: "FK_AssetTransferIssueHdr_AssetTransferReceiptHdr_AssetTransferReceiptHdrId1",
                    schema: "FixedAsset",
                    table: "AssetTransferIssueHdr");

                // Drop the index
                migrationBuilder.DropIndex(
                    name: "IX_AssetTransferIssueHdr_AssetTransferReceiptHdrId1",
                    schema: "FixedAsset",
                    table: "AssetTransferIssueHdr");

                // Drop the column
                migrationBuilder.DropColumn(
                    name: "AssetTransferReceiptHdrId1",
                    schema: "FixedAsset",
                    table: "AssetTransferIssueHdr");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
                        // Re-add the column
                migrationBuilder.AddColumn<int>(
                    name: "AssetTransferReceiptHdrId1",
                    schema: "FixedAsset",
                    table: "AssetTransferIssueHdr",
                    type: "int",
                    nullable: true);

                // Recreate the index
                migrationBuilder.CreateIndex(
                    name: "IX_AssetTransferIssueHdr_AssetTransferReceiptHdrId1",
                    schema: "FixedAsset",
                    table: "AssetTransferIssueHdr",
                    column: "AssetTransferReceiptHdrId1");

                // Recreate the foreign key constraint
                migrationBuilder.AddForeignKey(
                    name: "FK_AssetTransferIssueHdr_AssetTransferReceiptHdr_AssetTransferReceiptHdrId1",
                    schema: "FixedAsset",
                    table: "AssetTransferIssueHdr",
                    column: "AssetTransferReceiptHdrId1",
                    principalSchema: "FixedAsset",
                    principalTable: "AssetTransferReceiptHdr",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
        }
    }
}
