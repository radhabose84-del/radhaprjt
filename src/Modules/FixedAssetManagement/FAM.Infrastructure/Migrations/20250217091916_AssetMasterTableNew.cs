using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetMasterTableNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMasterGenerals_AssetCategories_AssetCategoriesId",
                table: "AssetMasterGenerals");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetMasterGenerals_AssetGroup_AssetGroupId",
                table: "AssetMasterGenerals");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetMasterGenerals_AssetMasterGenerals_AssetParentId",
                table: "AssetMasterGenerals");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetMasterGenerals_AssetSubCategories_AssetSubCategoriesId",
                table: "AssetMasterGenerals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssetMasterGenerals",
                table: "AssetMasterGenerals");

            migrationBuilder.DropIndex(
                name: "IX_AssetMasterGenerals_AssetCategoriesId",
                table: "AssetMasterGenerals");

            migrationBuilder.DropIndex(
                name: "IX_AssetMasterGenerals_AssetSubCategoriesId",
                table: "AssetMasterGenerals");

            migrationBuilder.DropColumn(
                name: "AssetCategoriesId",
                table: "AssetMasterGenerals");

            migrationBuilder.DropColumn(
                name: "AssetSubCategoriesId",
                table: "AssetMasterGenerals");

            migrationBuilder.RenameTable(
                name: "AssetMasterGenerals",
                newName: "AssetMaster",
                newSchema: "FixedAsset");

            migrationBuilder.RenameIndex(
                name: "IX_AssetMasterGenerals_AssetParentId",
                schema: "FixedAsset",
                table: "AssetMaster",
                newName: "IX_AssetMaster_AssetParentId");

            migrationBuilder.RenameIndex(
                name: "IX_AssetMasterGenerals_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetMaster",
                newName: "IX_AssetMaster_AssetGroupId");

            migrationBuilder.AlterColumn<string>(
                name: "WorkingStatus",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UOMId",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MachineCode",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsTangible",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "ISDepreciated",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssetType",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(10)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssetName",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssetImage",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "nvarchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssetDescription",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(250)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssetCode",
                schema: "FixedAsset",
                table: "AssetMaster",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssetMaster",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_AssetCategoryId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMaster_AssetSubCategoryId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetSubCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_AssetCategories_AssetCategoryId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetCategoryId",
                principalSchema: "FixedAsset",
                principalTable: "AssetCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_AssetMaster_AssetParentId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetParentId",
                principalSchema: "FixedAsset",
                principalTable: "AssetMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMaster_AssetSubCategories_AssetSubCategoryId",
                schema: "FixedAsset",
                table: "AssetMaster",
                column: "AssetSubCategoryId",
                principalSchema: "FixedAsset",
                principalTable: "AssetSubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_AssetCategories_AssetCategoryId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_AssetMaster_AssetParentId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetMaster_AssetSubCategories_AssetSubCategoryId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssetMaster",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_AssetCategoryId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.DropIndex(
                name: "IX_AssetMaster_AssetSubCategoryId",
                schema: "FixedAsset",
                table: "AssetMaster");

            migrationBuilder.RenameTable(
                name: "AssetMaster",
                schema: "FixedAsset",
                newName: "AssetMasterGenerals");

            migrationBuilder.RenameIndex(
                name: "IX_AssetMaster_AssetParentId",
                table: "AssetMasterGenerals",
                newName: "IX_AssetMasterGenerals_AssetParentId");

            migrationBuilder.RenameIndex(
                name: "IX_AssetMaster_AssetGroupId",
                table: "AssetMasterGenerals",
                newName: "IX_AssetMasterGenerals_AssetGroupId");

            migrationBuilder.AlterColumn<string>(
                name: "WorkingStatus",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<int>(
                name: "UOMId",
                table: "AssetMasterGenerals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "AssetMasterGenerals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MachineCode",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<byte>(
                name: "IsTangible",
                table: "AssetMasterGenerals",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "IsDeleted",
                table: "AssetMasterGenerals",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "IsActive",
                table: "AssetMasterGenerals",
                type: "int",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<byte>(
                name: "ISDepreciated",
                table: "AssetMasterGenerals",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "AssetType",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<string>(
                name: "AssetName",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "AssetImage",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AssetDescription",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(250)");

            migrationBuilder.AlterColumn<string>(
                name: "AssetCode",
                table: "AssetMasterGenerals",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AddColumn<int>(
                name: "AssetCategoriesId",
                table: "AssetMasterGenerals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AssetSubCategoriesId",
                table: "AssetMasterGenerals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssetMasterGenerals",
                table: "AssetMasterGenerals",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMasterGenerals_AssetCategoriesId",
                table: "AssetMasterGenerals",
                column: "AssetCategoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMasterGenerals_AssetSubCategoriesId",
                table: "AssetMasterGenerals",
                column: "AssetSubCategoriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMasterGenerals_AssetCategories_AssetCategoriesId",
                table: "AssetMasterGenerals",
                column: "AssetCategoriesId",
                principalSchema: "FixedAsset",
                principalTable: "AssetCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMasterGenerals_AssetGroup_AssetGroupId",
                table: "AssetMasterGenerals",
                column: "AssetGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMasterGenerals_AssetMasterGenerals_AssetParentId",
                table: "AssetMasterGenerals",
                column: "AssetParentId",
                principalTable: "AssetMasterGenerals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetMasterGenerals_AssetSubCategories_AssetSubCategoriesId",
                table: "AssetMasterGenerals",
                column: "AssetSubCategoriesId",
                principalSchema: "FixedAsset",
                principalTable: "AssetSubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
