using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetTransferReceiptTableMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetTransferReceiptHdrId1",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetTransferReceiptHdr",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetTransferId = table.Column<int>(type: "int", nullable: false),
                    DocDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TransferType = table.Column<int>(type: "int", nullable: false),
                    FromUnitId = table.Column<int>(type: "int", nullable: false),
                    ToUnitId = table.Column<int>(type: "int", nullable: false),
                    FromDepartmentId = table.Column<int>(type: "int", nullable: false),
                    ToDepartmentId = table.Column<int>(type: "int", nullable: false),
                    FromCustodianId = table.Column<int>(type: "int", nullable: false),
                    FromCustodianName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    ToCustodianId = table.Column<int>(type: "int", nullable: false),
                    ToCustodianName = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    Sdcno = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    GatePassNo = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    AuthorizedBy = table.Column<string>(type: "varchar(50)", nullable: false),
                    AuthorizedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AuthorizedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    AuthorizedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(250)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTransferReceiptHdr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTransferReceiptHdr_AssetTransferIssueHdr_AssetTransferId",
                        column: x => x.AssetTransferId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetTransferIssueHdr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTransferReceiptHdr_MiscMaster_TransferType",
                        column: x => x.TransferType,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

          

            migrationBuilder.CreateTable(
                name: "AssetTransferReceiptDtl",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetReceiptId = table.Column<int>(type: "int", nullable: false),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    SubLocationId = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetTransferReceiptDtl", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetTransferReceiptDtl_AssetMaster_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTransferReceiptDtl_AssetTransferReceiptHdr_AssetReceiptId",
                        column: x => x.AssetReceiptId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetTransferReceiptHdr",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetTransferReceiptDtl_Location_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "FixedAsset",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetTransferReceiptDtl_SubLocation_SubLocationId",
                        column: x => x.SubLocationId,
                        principalSchema: "FixedAsset",
                        principalTable: "SubLocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferIssueHdr_AssetTransferReceiptHdrId1",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr",
                column: "AssetTransferReceiptHdrId1");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferReceiptDtl_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptDtl",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferReceiptDtl_AssetReceiptId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptDtl",
                column: "AssetReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferReceiptDtl_LocationId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptDtl",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferReceiptDtl_SubLocationId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptDtl",
                column: "SubLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferReceiptHdr_AssetTransferId",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                column: "AssetTransferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferReceiptHdr_TransferType",
                schema: "FixedAsset",
                table: "AssetTransferReceiptHdr",
                column: "TransferType");


            migrationBuilder.AddForeignKey(
                name: "FK_AssetTransferIssueHdr_AssetTransferReceiptHdr_AssetTransferReceiptHdrId1",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr",
                column: "AssetTransferReceiptHdrId1",
                principalSchema: "FixedAsset",
                principalTable: "AssetTransferReceiptHdr",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetTransferIssueHdr_AssetTransferReceiptHdr_AssetTransferReceiptHdrId1",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr");

            migrationBuilder.DropTable(
                name: "AssetTransferReceiptDtl",
                schema: "FixedAsset");


            migrationBuilder.DropTable(
                name: "AssetTransferReceiptHdr",
                schema: "FixedAsset");

            migrationBuilder.DropIndex(
                name: "IX_AssetTransferIssueHdr_AssetTransferReceiptHdrId1",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr");

            migrationBuilder.DropColumn(
                name: "AssetTransferReceiptHdrId1",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr");
        }
    }
}
