using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Assettransferassetiduniqueremove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_AssetTransferIssueDtl_AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetTransferIssueDtl_AssetMaster_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl");

        

            migrationBuilder.DropIndex(
                name: "IX_AssetTransferIssueDtl_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropColumn(
                name: "AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

          

            migrationBuilder.AddColumn<string>(
                name: "FromCustodianName",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ToCustodianName",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr",
                type: "nvarchar(100)",
                nullable: false,
                defaultValue: "");        

           

            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferIssueDtl_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl",
                column: "AssetId");
          

            migrationBuilder.AddForeignKey(
                name: "FK_AssetTransferIssueDtl_AssetMaster_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl",
                column: "AssetId",
                principalSchema: "FixedAsset",
                principalTable: "AssetMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

       
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetTransferIssueDtl_AssetMaster_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl");

          
            migrationBuilder.DropIndex(
                name: "IX_AssetTransferIssueDtl_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl");

           

            migrationBuilder.DropColumn(
                name: "FromCustodianName",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr");

            migrationBuilder.DropColumn(
                name: "ToCustodianName",
                schema: "FixedAsset",
                table: "AssetTransferIssueHdr");

          

            migrationBuilder.AddColumn<int>(
                name: "AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true);

         
            migrationBuilder.CreateIndex(
                name: "IX_AssetTransferIssueDtl_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl",
                column: "AssetId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetTransferIssueMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_AssetTransferIssueDtl_AssetTransferIssueMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetTransferIssueMasterId",
                principalSchema: "FixedAsset",
                principalTable: "AssetTransferIssueDtl",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetTransferIssueDtl_AssetMaster_AssetId",
                schema: "FixedAsset",
                table: "AssetTransferIssueDtl",
                column: "AssetId",
                principalSchema: "FixedAsset",
                principalTable: "AssetMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
         
        }
    }
}
