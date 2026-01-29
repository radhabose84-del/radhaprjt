using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetMasterMiscType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_DepreciationGroups_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Manufacture_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "Manufacture");

            migrationBuilder.DropIndex(
                name: "IX_Manufacture_MiscMasterId",
                schema: "FixedAsset",
                table: "Manufacture");

            migrationBuilder.DropIndex(
                name: "IX_DepreciationGroups_MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "Manufacture");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.AlterColumn<int>(
                name: "ManufactureType",
                schema: "FixedAsset",
                table: "Manufacture",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DepreciationMethod",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BookType",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "WorkingStatus",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AssetType",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ManufactureType",
                schema: "FixedAsset",
                table: "Manufacture",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "Manufacture",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DepreciationMethod",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BookType",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WorkingStatus",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssetType",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Manufacture_MiscMasterId",
                schema: "FixedAsset",
                table: "Manufacture",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationGroups_MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "MiscMasterId",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepreciationGroups_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "MiscMasterId",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Manufacture_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "Manufacture",
                column: "MiscMasterId",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id");
        }
    }
}
