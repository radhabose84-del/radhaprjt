using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetMasterGeneral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "DepreciationMethod",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<int>(
                name: "BookType",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AddColumn<int>(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetMasterGenerals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    AssetCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssetGroupId = table.Column<int>(type: "int", nullable: false),
                    AssetCategoryId = table.Column<int>(type: "int", nullable: false),
                    AssetCategoriesId = table.Column<int>(type: "int", nullable: false),
                    AssetSubCategoryId = table.Column<int>(type: "int", nullable: false),
                    AssetSubCategoriesId = table.Column<int>(type: "int", nullable: false),
                    AssetParentId = table.Column<int>(type: "int", nullable: true),
                    AssetType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MachineCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: true),
                    UOMId = table.Column<int>(type: "int", nullable: true),
                    AssetDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkingStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssetImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ISDepreciated = table.Column<byte>(type: "tinyint", nullable: false),
                    IsTangible = table.Column<byte>(type: "tinyint", nullable: false),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetMasterGenerals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetMasterGenerals_AssetCategories_AssetCategoriesId",
                        column: x => x.AssetCategoriesId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetMasterGenerals_AssetGroup_AssetGroupId",
                        column: x => x.AssetGroupId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetMasterGenerals_AssetMasterGenerals_AssetParentId",
                        column: x => x.AssetParentId,
                        principalTable: "AssetMasterGenerals",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetMasterGenerals_AssetSubCategories_AssetSubCategoriesId",
                        column: x => x.AssetSubCategoriesId,
                        principalSchema: "FixedAsset",
                        principalTable: "AssetSubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
           

            migrationBuilder.CreateTable(
                name: "Manufacture",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "varchar(10)", nullable: false),
                    ManufactureName = table.Column<string>(type: "varchar(50)", nullable: false),
                    ManufactureType = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    StateId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    AddressLine1 = table.Column<string>(type: "varchar(250)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "varchar(250)", nullable: false),
                    PinCode = table.Column<string>(type: "varchar(10)", nullable: false),
                    PersonName = table.Column<string>(type: "varchar(50)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", nullable: false),
                    Email = table.Column<string>(type: "varchar(50)", nullable: false),
                    MiscMasterId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_Manufacture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManufactureType_Misc",
                        column: x => x.ManufactureType,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Manufacture_MiscMaster_MiscMasterId",
                        column: x => x.MiscMasterId,
                        principalSchema: "FixedAsset",
                        principalTable: "MiscMaster",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationGroups_BookType",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "BookType");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationGroups_DepreciationMethod",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "DepreciationMethod");

            migrationBuilder.CreateIndex(
                name: "IX_DepreciationGroups_MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "MiscMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMasterGenerals_AssetCategoriesId",
                table: "AssetMasterGenerals",
                column: "AssetCategoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMasterGenerals_AssetGroupId",
                table: "AssetMasterGenerals",
                column: "AssetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMasterGenerals_AssetParentId",
                table: "AssetMasterGenerals",
                column: "AssetParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetMasterGenerals_AssetSubCategoriesId",
                table: "AssetMasterGenerals",
                column: "AssetSubCategoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Manufacture_ManufactureType",
                schema: "FixedAsset",
                table: "Manufacture",
                column: "ManufactureType");

            migrationBuilder.CreateIndex(
                name: "IX_Manufacture_MiscMasterId",
                schema: "FixedAsset",
                table: "Manufacture",
                column: "MiscMasterId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookType_Misc",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "BookType",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DepreciationGroups_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "MiscMasterId",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DepreciationMethod_Misc",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                column: "DepreciationMethod",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookType_Misc",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_DepreciationGroups_MiscMaster_MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_DepreciationMethod_Misc",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.DropTable(
                name: "AssetMasterGenerals");

            migrationBuilder.DropTable(
                name: "Manufacture",
                schema: "FixedAsset");

            migrationBuilder.DropTable(
                name: "MiscMaster",
                schema: "FixedAsset");

            migrationBuilder.DropIndex(
                name: "IX_DepreciationGroups_BookType",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.DropIndex(
                name: "IX_DepreciationGroups_DepreciationMethod",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.DropIndex(
                name: "IX_DepreciationGroups_MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.DropColumn(
                name: "MiscMasterId",
                schema: "FixedAsset",
                table: "DepreciationGroups");

            migrationBuilder.AlterColumn<string>(
                name: "DepreciationMethod",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "BookType",
                schema: "FixedAsset",
                table: "DepreciationGroups",
                type: "varchar(50)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
