using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetinsurancetblInsurancebaseentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByName",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedDate",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedIP",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "varchar(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedByName",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "varchar(50)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ModifiedDate",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedIP",
                schema: "FixedAsset",
                table: "AssetInsurance",
                type: "varchar(20)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "FixedAsset",
                table: "AssetInsurance");

            migrationBuilder.DropColumn(
                name: "CreatedByName",
                schema: "FixedAsset",
                table: "AssetInsurance");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "FixedAsset",
                table: "AssetInsurance");

            migrationBuilder.DropColumn(
                name: "CreatedIP",
                schema: "FixedAsset",
                table: "AssetInsurance");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "FixedAsset",
                table: "AssetInsurance");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "FixedAsset",
                table: "AssetInsurance");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "FixedAsset",
                table: "AssetInsurance");

            migrationBuilder.DropColumn(
                name: "ModifiedByName",
                schema: "FixedAsset",
                table: "AssetInsurance");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                schema: "FixedAsset",
                table: "AssetInsurance");

            migrationBuilder.DropColumn(
                name: "ModifiedIP",
                schema: "FixedAsset",
                table: "AssetInsurance");
        }
    }
}
