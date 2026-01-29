using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetAudittableChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetCondition",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "AuditAssetCode",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "AuditCustodian",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "AuditDepartment",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "AuditLocation",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "AuditPeriod",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "AuditSubLocation",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "AuditUnitName",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "Custodian",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "GroupName",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "SubCategoryName",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.RenameColumn(
                name: "SubLocation",
                schema: "FixedAsset",
                table: "AssetAudit",
                newName: "AuditFinancialYear");

            migrationBuilder.AddColumn<int>(
                name: "AuditTypeId",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ScanType",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(10)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UploadedFileId",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetAudit_AuditTypeId",
                schema: "FixedAsset",
                table: "AssetAudit",
                column: "AuditTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAudit_MiscMaster_AuditTypeId",
                schema: "FixedAsset",
                table: "AssetAudit",
                column: "AuditTypeId",
                principalSchema: "FixedAsset",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetAudit_MiscMaster_AuditTypeId",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropIndex(
                name: "IX_AssetAudit_AuditTypeId",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "AuditTypeId",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "ScanType",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "UnitId",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.DropColumn(
                name: "UploadedFileId",
                schema: "FixedAsset",
                table: "AssetAudit");

            migrationBuilder.RenameColumn(
                name: "AuditFinancialYear",
                schema: "FixedAsset",
                table: "AssetAudit",
                newName: "SubLocation");

            migrationBuilder.AddColumn<string>(
                name: "AssetCondition",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditAssetCode",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditCustodian",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditDepartment",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditLocation",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditPeriod",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditSubLocation",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditUnitName",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Custodian",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubCategoryName",
                schema: "FixedAsset",
                table: "AssetAudit",
                type: "varchar(100)",
                nullable: true);
        }
    }
}
