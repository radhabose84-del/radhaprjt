using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AssetAuditTableMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetAudit",
                schema: "FixedAsset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "varchar(100)", nullable: true),
                    UnitName = table.Column<string>(type: "varchar(100)", nullable: true),
                    AssetCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    AssetName = table.Column<string>(type: "varchar(100)", nullable: true),
                    GroupName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CategoryName = table.Column<string>(type: "varchar(100)", nullable: true),
                    SubCategoryName = table.Column<string>(type: "varchar(100)", nullable: true),
                    Department = table.Column<string>(type: "varchar(100)", nullable: true),
                    Location = table.Column<string>(type: "varchar(100)", nullable: true),
                    SubLocation = table.Column<string>(type: "varchar(100)", nullable: true),
                    Custodian = table.Column<string>(type: "varchar(100)", nullable: true),
                    AuditAssetCode = table.Column<string>(type: "varchar(100)", nullable: true),
                    AuditUnitName = table.Column<string>(type: "varchar(100)", nullable: true),
                    AuditDepartment = table.Column<string>(type: "varchar(100)", nullable: true),
                    AuditLocation = table.Column<string>(type: "varchar(100)", nullable: true),
                    AuditSubLocation = table.Column<string>(type: "varchar(100)", nullable: true),
                    AuditCustodian = table.Column<string>(type: "varchar(100)", nullable: true),
                    AssetCondition = table.Column<string>(type: "varchar(100)", nullable: true),
                    AuditorName = table.Column<string>(type: "varchar(100)", nullable: true),
                    AuditDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AuditPeriod = table.Column<string>(type: "varchar(100)", nullable: true),
                    SourceFileName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedByName = table.Column<string>(type: "varchar(50)", nullable: false),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetAudit", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetAudit",
                schema: "FixedAsset");
        }
    }
}
