using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GstrSectionMasterAndLinkage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GstrSectionMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    ReportTypeId = table.Column<int>(type: "int", nullable: false),
                    SectionCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(200)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GstrSectionMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GstrSectionMaster_MiscMaster_ReportTypeId",
                        column: x => x.ReportTypeId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GstrSectionAccountLinkage",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionMasterId = table.Column<int>(type: "int", nullable: false),
                    AccountRangeFrom = table.Column<string>(type: "varchar(20)", nullable: false),
                    AccountRangeTo = table.Column<string>(type: "varchar(20)", nullable: false),
                    DerivedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExpectedValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TolerancePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 1m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    CreatedIP = table.Column<string>(type: "varchar(50)", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedByName = table.Column<string>(type: "varchar(100)", nullable: true),
                    ModifiedIP = table.Column<string>(type: "varchar(50)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GstrSectionAccountLinkage", x => x.Id);
                    table.CheckConstraint("CK_GSAL_Tolerance", "[TolerancePercent] >= 0 AND [TolerancePercent] <= 100");
                    table.ForeignKey(
                        name: "FK_GstrSectionAccountLinkage_GstrSectionMaster_SectionMasterId",
                        column: x => x.SectionMasterId,
                        principalSchema: "Finance",
                        principalTable: "GstrSectionMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TaxAccountLinkage_OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "OldControlAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxAccountLinkage_OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "OldTaxCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_GstrSectionAccountLinkage_SectionMasterId",
                schema: "Finance",
                table: "GstrSectionAccountLinkage",
                column: "SectionMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_GstrSectionMaster_ReportTypeId",
                schema: "Finance",
                table: "GstrSectionMaster",
                column: "ReportTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_GstrSectionMaster_Company_Report_Code",
                schema: "Finance",
                table: "GstrSectionMaster",
                columns: new[] { "CompanyId", "ReportTypeId", "SectionCode" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "OldControlAccountId",
                principalSchema: "Finance",
                principalTable: "MiscMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxAccountLinkage_TaxCodeMaster_OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "OldTaxCodeId",
                principalSchema: "Finance",
                principalTable: "TaxCodeMaster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaxAccountLinkage_MiscMaster_OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxAccountLinkage_TaxCodeMaster_OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropTable(
                name: "GstrSectionAccountLinkage",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "GstrSectionMaster",
                schema: "Finance");

            migrationBuilder.DropIndex(
                name: "IX_TaxAccountLinkage_OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropIndex(
                name: "IX_TaxAccountLinkage_OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropColumn(
                name: "OldControlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage");

            migrationBuilder.DropColumn(
                name: "OldTaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage");
        }
    }
}
