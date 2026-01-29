using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetTransferTablesmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetTransferIssueHdr",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TransferType = table.Column<int>(type: "int", nullable: false),
                    FromUnitId = table.Column<int>(type: "int", nullable: false),
                    ToUnitId = table.Column<int>(type: "int", nullable: false),
                    FromDepartmentId = table.Column<int>(type: "int", nullable: false),
                    ToDepartmentId = table.Column<int>(type: "int", nullable: false),
                    FromCustodianId = table.Column<int>(type: "int", nullable: false),
                    ToCustodianId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModifiedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    AuthorizedBy = table.Column<string>(type: "varchar(50)", nullable: true),
                    AuthorizedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    AuthorizedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    AuthorizedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTransferIssueHdr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTransferIssueHdr_MiscMaster_TransferType",
                        column: x => x.TransferType,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssetTransferIssueDtl",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetTransferId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    AssetValue = table.Column<decimal>(type: "decimal(18,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTransferIssueDtl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTransferIssueDtl_AssetMaster_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTransferIssueDtl_AssetTransferIssueHdr_AssetTransferId",
                        column: x => x.AssetTransferId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetTransferIssueHdr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetTransferIssueMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferIssueDtl_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl",
                column: "AssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferIssueDtl_AssetTransferId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl",
                column: "AssetTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferIssueHdr_TransferType",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr",
                column: "TransferType");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_AssetTransferIssueDtl_AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetTransferIssueMasterId",
                principalSchema: "FixedAsset",
                principalTable: "AssetTransferIssueDtl",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_AssetTransferIssueDtl_AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropTable(
                name: "AssetTransferIssueDtl",
                schema: "FixedAsset");

            migrationBuilder.DropTable(
                name: "AssetTransferIssueHdr",
                schema: "FixedAsset");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropColumn(
                name: "AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");
        }
    }
}
