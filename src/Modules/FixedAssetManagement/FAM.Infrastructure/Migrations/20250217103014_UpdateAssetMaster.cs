using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssetMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WorkingStatus",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<int>(
                name: "AssetType",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_AssetType",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetType");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_WorkingStatus",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "WorkingStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "MiscMasterId",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetType_Misc",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetType",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkingStatus_Misc",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "WorkingStatus",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetType_Misc",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkingStatus_Misc",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_AssetType",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_WorkingStatus",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.AlterColumn<string>(
                name: "WorkingStatus",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AssetType",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
