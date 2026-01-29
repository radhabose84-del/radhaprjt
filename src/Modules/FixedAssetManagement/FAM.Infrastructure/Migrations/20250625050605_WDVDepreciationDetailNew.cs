using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WDVDepreciationDetailNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WDVDepreciationDetail_AssetGroup_AssetGroupId",
                table: "WDVDepreciationDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WDVDepreciationDetail_AssetSubGroup_AssetSubGroupId",
                table: "WDVDepreciationDetail");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "WDVDepreciationDetail");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "WDVDepreciationDetail");

            migrationBuilder.RenameTable(
                name: "WDVDepreciationDetail",
                newName: "WDVDepreciationDetail",
                newSchema: "FixedAsset");

            migrationBuilder.AlterColumn<decimal>(
                name: "WDVDepreciationValue",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "StartDate",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningValue",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MoreThan180DaysValue",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "varchar(50)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LessThan180DaysValue",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsLocked",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<int>(
                name: "FinYear",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EndDate",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "DepreciationValue",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DepreciationPercentage",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DeletionValue",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ClosingValue",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CapitalGainLossValue",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AdditionalDepreciationValue",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_WDVDepreciationDetail_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                column: "AssetGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WDVDepreciationDetail_AssetSubGroup_AssetSubGroupId",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail",
                column: "AssetSubGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetSubGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WDVDepreciationDetail_AssetGroup_AssetGroupId",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_WDVDepreciationDetail_AssetSubGroup_AssetSubGroupId",
                schema: "FixedAsset",
                table: "WDVDepreciationDetail");

            migrationBuilder.RenameTable(
                name: "WDVDepreciationDetail",
                schema: "FixedAsset",
                newName: "WDVDepreciationDetail");

            migrationBuilder.AlterColumn<decimal>(
                name: "WDVDepreciationValue",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "StartDate",
                table: "WDVDepreciationDetail",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<decimal>(
                name: "OpeningValue",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MoreThan180DaysValue",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedIP",
                table: "WDVDepreciationDetail",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedByName",
                table: "WDVDepreciationDetail",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LessThan180DaysValue",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<byte>(
                name: "IsLocked",
                table: "WDVDepreciationDetail",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "FinYear",
                table: "WDVDepreciationDetail",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EndDate",
                table: "WDVDepreciationDetail",
                type: "datetimeoffset",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<decimal>(
                name: "DepreciationValue",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DepreciationPercentage",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DeletionValue",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedIP",
                table: "WDVDepreciationDetail",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByName",
                table: "WDVDepreciationDetail",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)");

            migrationBuilder.AlterColumn<decimal>(
                name: "ClosingValue",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CapitalGainLossValue",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "AdditionalDepreciationValue",
                table: "WDVDepreciationDetail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");

            migrationBuilder.AddColumn<int>(
                name: "IsActive",
                table: "WDVDepreciationDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IsDeleted",
                table: "WDVDepreciationDetail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_WDVDepreciationDetail_AssetGroup_AssetGroupId",
                table: "WDVDepreciationDetail",
                column: "AssetGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WDVDepreciationDetail_AssetSubGroup_AssetSubGroupId",
                table: "WDVDepreciationDetail",
                column: "AssetSubGroupId",
                principalSchema: "FixedAsset",
                principalTable: "AssetSubGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
