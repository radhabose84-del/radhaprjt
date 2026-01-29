using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetDisposalTablemigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

                migrationBuilder.AddColumn<int>(
                name: "AssetMiscDisposalTypeId",
                schema: "FixedAsset",
                table: "MiscMaster",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetDisposal",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    AssetPurchaseId = table.Column<int>(type: "int", nullable: false),
                    DisposalDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DisposalType = table.Column<int>(type: "int", nullable: false),
                    DisposalReason = table.Column<string>(type: "nvarchar(250)", nullable: true),
                    DisposalAmount = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetDisposal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetDisposal_AssetMaster_AssetId",
                        column: x => x.AssetId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetDisposal_AssetPurchaseDetails_AssetPurchaseId",
                        column: x => x.AssetPurchaseId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetPurchaseDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssetDisposal_MiscMaster_DisposalType",
                        column: x => x.DisposalType,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MiscMaster_AssetMiscDisposalTypeId",
                schema: "FixedAsset",
                table: "MiscMaster",
                column: "AssetMiscDisposalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetPurchaseDetails_AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "AssetDisposalPurchaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetDisposalMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetDisposal_AssetId",
                schema: "FixedAsset",
                table: "AssetDisposal",
                column: "AssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetDisposal_AssetPurchaseId",
                schema: "FixedAsset",
                table: "AssetDisposal",
                column: "AssetPurchaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetDisposal_DisposalType",
                schema: "FixedAsset",
                table: "AssetDisposal",
                column: "DisposalType",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_AssetDisposal_AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetDisposalMasterId",
                principalSchema: "FixedAsset",
                principalTable: "AssetDisposal",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetPurchaseDetails_AssetDisposal_AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails",
                column: "AssetDisposalPurchaseId",
                principalSchema: "FixedAsset",
                principalTable: "AssetDisposal",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MiscMaster_AssetDisposal_AssetMiscDisposalTypeId",
                schema: "FixedAsset",
                table: "MiscMaster",
                column: "AssetMiscDisposalTypeId",
                principalSchema: "FixedAsset",
                principalTable: "AssetDisposal",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_AssetDisposal_AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetPurchaseDetails_AssetDisposal_AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_MiscMaster_AssetDisposal_AssetMiscDisposalTypeId",
                schema: "FixedAsset",
                table: "MiscMaster");

            migrationBuilder.DropTable(
                name: "AssetDisposal",
                schema: "FixedAsset");

            migrationBuilder.DropIndex(
                name: "IX_MiscMaster_AssetMiscDisposalTypeId",
                schema: "FixedAsset",
                table: "MiscMaster");

            migrationBuilder.DropIndex(
                name: "IX_AssetPurchaseDetails_AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropColumn(
                name: "AssetMiscDisposalTypeId",
                schema: "FixedAsset",
                table: "MiscMaster");

            migrationBuilder.DropColumn(
                name: "AssetDisposalPurchaseId",
                schema: "FixedAsset",
                table: "AssetPurchaseDetails");

            migrationBuilder.DropColumn(
                name: "AssetDisposalMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

           
        }
    }
}
