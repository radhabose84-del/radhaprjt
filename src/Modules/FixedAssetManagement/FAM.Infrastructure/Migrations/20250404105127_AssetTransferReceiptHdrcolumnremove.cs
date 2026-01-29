using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetTransferReceiptHdrcolumnremove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetTransferReceiptHdr_MiscMaster_TransferType",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropIndex(
                name: "IX_AssetTransferReceiptHdr_TransferType",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropColumn(
                name: "FromCustodianId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropColumn(
                name: "FromCustodianName",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropColumn(
                name: "FromDepartmentId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropColumn(
                name: "FromUnitId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropColumn(
                name: "ToCustodianId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropColumn(
                name: "ToCustodianName",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropColumn(
                name: "ToDepartmentId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropColumn(
                name: "ToUnitId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");

            migrationBuilder.DropColumn(
                name: "TransferType",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FromCustodianId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FromCustodianName",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FromDepartmentId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FromUnitId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToCustodianId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ToCustodianName",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ToDepartmentId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ToUnitId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransferType",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferReceiptHdr_TransferType",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                column: "TransferType");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetTransferReceiptHdr_MiscMaster_TransferType",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                column: "TransferType",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
