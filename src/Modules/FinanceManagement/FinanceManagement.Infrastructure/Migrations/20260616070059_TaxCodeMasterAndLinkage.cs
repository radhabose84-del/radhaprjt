using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaxCodeMasterAndLinkage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GstrSectionMapping",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    GstrType = table.Column<string>(type: "varchar(10)", nullable: false),
                    SectionCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    SectionName = table.Column<string>(type: "varchar(150)", nullable: false),
                    AccountRangeFrom = table.Column<string>(type: "varchar(20)", nullable: false),
                    AccountRangeTo = table.Column<string>(type: "varchar(20)", nullable: false),
                    TolerancePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_GstrSectionMapping", x => x.Id);
                    table.CheckConstraint("CK_GSM_Tolerance", "[TolerancePercent] IS NULL OR [TolerancePercent] >= 0");
                    table.CheckConstraint("CK_GSM_Type", "[GstrType] IN ('GSTR1','GSTR3B')");
                });

            migrationBuilder.CreateTable(
                name: "TaxCodeMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TaxCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    TaxName = table.Column<string>(type: "varchar(150)", nullable: false),
                    TaxType = table.Column<string>(type: "varchar(15)", nullable: false),
                    TaxComponent = table.Column<string>(type: "varchar(10)", nullable: false, defaultValue: "COMBINED"),
                    ParentTaxCodeId = table.Column<int>(type: "int", nullable: true),
                    Direction = table.Column<string>(type: "varchar(10)", nullable: true),
                    StatutorySection = table.Column<string>(type: "varchar(20)", nullable: true),
                    ThresholdAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ThresholdAggregate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HsnSacCode = table.Column<string>(type: "varchar(10)", nullable: true),
                    IsSystemOnlyPosting = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsEefcRelevant = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsStatutoryFixed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_TaxCodeMaster", x => x.Id);
                    table.CheckConstraint("CK_TCM_Component", "[TaxComponent] IN ('COMBINED','CGST','SGST','IGST','CESS','NA')");
                    table.CheckConstraint("CK_TCM_Direction", "[Direction] IS NULL OR [Direction] IN ('INPUT','OUTPUT')");
                    table.CheckConstraint("CK_TCM_ParentLink", "([TaxComponent] IN ('CGST','SGST','IGST','CESS') AND [ParentTaxCodeId] IS NOT NULL) OR ([TaxComponent] IN ('COMBINED','NA') AND [ParentTaxCodeId] IS NULL)");
                    table.CheckConstraint("CK_TCM_TaxType", "[TaxType] IN ('GST_IN','GST_OUT','IGST','TDS','CUSTOMS')");
                    table.CheckConstraint("CK_TCM_Threshold", "[ThresholdAmount] IS NULL OR [ThresholdAmount] >= 0");
                    table.CheckConstraint("CK_TCM_ThresholdAgg", "[ThresholdAggregate] IS NULL OR [ThresholdAggregate] >= 0");
                    table.ForeignKey(
                        name: "FK_TaxCodeMaster_TaxCodeMaster_ParentTaxCodeId",
                        column: x => x.ParentTaxCodeId,
                        principalSchema: "Finance",
                        principalTable: "TaxCodeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxAccountLinkage",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TaxCodeId = table.Column<int>(type: "int", nullable: false),
                    GlAccountId = table.Column<int>(type: "int", nullable: false),
                    IsActivated = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ApprovalStatus = table.Column<string>(type: "varchar(15)", nullable: false, defaultValue: "PENDING"),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_TaxAccountLinkage", x => x.Id);
                    table.CheckConstraint("CK_TAL_Dates", "[EffectiveTo] IS NULL OR [EffectiveTo] > [EffectiveFrom]");
                    table.CheckConstraint("CK_TAL_Status", "[ApprovalStatus] IN ('PENDING','APPROVED','REJECTED')");
                    table.ForeignKey(
                        name: "FK_TaxAccountLinkage_GlAccountMaster_GlAccountId",
                        column: x => x.GlAccountId,
                        principalSchema: "Finance",
                        principalTable: "GlAccountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaxAccountLinkage_TaxCodeMaster_TaxCodeId",
                        column: x => x.TaxCodeId,
                        principalSchema: "Finance",
                        principalTable: "TaxCodeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaxCodeRateVersion",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxCodeId = table.Column<int>(type: "int", nullable: false),
                    VersionNo = table.Column<int>(type: "int", nullable: false),
                    RatePercent = table.Column<decimal>(type: "decimal(7,4)", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    ChangeReason = table.Column<string>(type: "varchar(500)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_TaxCodeRateVersion", x => x.Id);
                    table.CheckConstraint("CK_TCRV_Dates", "[EffectiveTo] IS NULL OR [EffectiveTo] > [EffectiveFrom]");
                    table.CheckConstraint("CK_TCRV_Rate", "[RatePercent] >= 0 AND [RatePercent] <= 100");
                    table.ForeignKey(
                        name: "FK_TaxCodeRateVersion_TaxCodeMaster_TaxCodeId",
                        column: x => x.TaxCodeId,
                        principalSchema: "Finance",
                        principalTable: "TaxCodeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_GstrSectionMapping_Section",
                schema: "Finance",
                table: "GstrSectionMapping",
                columns: new[] { "CompanyId", "GstrType", "SectionCode" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaxAccountLinkage_GlAccountId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "GlAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxAccountLinkage_TaxCodeId",
                schema: "Finance",
                table: "TaxAccountLinkage",
                column: "TaxCodeId");

            migrationBuilder.CreateIndex(
                name: "UX_TaxAccountLinkage_ActivePerAccount",
                schema: "Finance",
                table: "TaxAccountLinkage",
                columns: new[] { "CompanyId", "GlAccountId" },
                unique: true,
                filter: "[EffectiveTo] IS NULL AND [ApprovalStatus] = 'APPROVED' AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeMaster_CompanyId",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeMaster_ParentTaxCodeId",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "ParentTaxCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeMaster_TaxType",
                schema: "Finance",
                table: "TaxCodeMaster",
                column: "TaxType");

            migrationBuilder.CreateIndex(
                name: "UX_TaxCodeMaster_Company_Code",
                schema: "Finance",
                table: "TaxCodeMaster",
                columns: new[] { "CompanyId", "TaxCode" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TaxCodeRateVersion_Code_From",
                schema: "Finance",
                table: "TaxCodeRateVersion",
                columns: new[] { "TaxCodeId", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "UQ_TCRV_Version",
                schema: "Finance",
                table: "TaxCodeRateVersion",
                columns: new[] { "TaxCodeId", "VersionNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TaxCodeRateVersion_OpenPerCode",
                schema: "Finance",
                table: "TaxCodeRateVersion",
                column: "TaxCodeId",
                unique: true,
                filter: "[EffectiveTo] IS NULL AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GstrSectionMapping",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "TaxAccountLinkage",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "TaxCodeRateVersion",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "TaxCodeMaster",
                schema: "Finance");
        }
    }
}
