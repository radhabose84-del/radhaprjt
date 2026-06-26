using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <summary>
    /// US-GL03-01..05 refactor (2026-06-26) — drops the parallel FinancialYearMaster + FinancialPeriodMaster
    /// model (those tables already removed via the manual cleanup script) and consolidates everything onto
    /// Finance.AccountingPeriod:
    ///   * 3 new columns: IsAdjustmentPeriod (Period 13 flag), LastStatusChangedBy, LastStatusChangedAt
    ///   * Filtered index for fast Period-13 lookup per FY
    /// Plus US-GL03-04 (Backdating Controls):
    ///   * 4 new JournalHeader columns: IsBackdated (persisted computed), BackdateReason,
    ///     BackdateAcknowledgedBy, BackdateAcknowledgedAt
    ///   * Composite index on (IsBackdated, IsDeleted, CompanyId)
    /// Plus US-GL03-02 (Period Status Override) creates a NEW PeriodStatusOverride table whose FK targets
    /// Finance.AccountingPeriod (the old PSO table referenced the dropped FinancialPeriodMaster).
    ///
    /// NOTE: The auto-generated migration also tried to add GlobalAccountId / IsCompanyRestricted /
    /// IsGlobal / IsLocalOverride to Finance.GlAccountMaster — those columns ALREADY exist in the DB
    /// (added by 20260625060653_MultiCompanyCoa) but were lost from the snapshot chain due to a Designer
    /// regression. We omit those operations here to avoid "column already exists" failures. The fresh
    /// snapshot regenerated alongside this migration correctly reflects their presence, so future
    /// migrations will diff against the right baseline.
    /// </summary>
    public partial class Refactor_PeriodModel_AndBackdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ─── AccountingPeriod — 3 new columns + 1 filtered index ─────────────────
            migrationBuilder.AddColumn<bool>(
                name: "IsAdjustmentPeriod",
                schema: "Finance",
                table: "AccountingPeriod",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastStatusChangedBy",
                schema: "Finance",
                table: "AccountingPeriod",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusChangedAt",
                schema: "Finance",
                table: "AccountingPeriod",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriod_AdjustmentPerFY",
                schema: "Finance",
                table: "AccountingPeriod",
                columns: new[] { "FinancialYearId", "IsAdjustmentPeriod" },
                filter: "[IsAdjustmentPeriod] = 1 AND [IsDeleted] = 0");

            // ─── JournalHeader — 4 new backdate columns + 1 composite index ──────────
            migrationBuilder.AddColumn<bool>(
                name: "IsBackdated",
                schema: "Finance",
                table: "JournalHeader",
                type: "bit",
                nullable: false,
                computedColumnSql: "CASE WHEN VoucherDate IS NULL OR PostedAt IS NULL THEN CAST(0 AS BIT) WHEN VoucherDate < CAST(PostedAt AS DATE) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "BackdateReason",
                schema: "Finance",
                table: "JournalHeader",
                type: "varchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BackdateAcknowledgedBy",
                schema: "Finance",
                table: "JournalHeader",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BackdateAcknowledgedAt",
                schema: "Finance",
                table: "JournalHeader",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_IsBackdated",
                schema: "Finance",
                table: "JournalHeader",
                columns: new[] { "IsBackdated", "IsDeleted", "CompanyId" });

            // ─── PeriodStatusOverride — new table targeting AccountingPeriod ─────────
            migrationBuilder.CreateTable(
                name: "PeriodStatusOverride",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountingPeriodId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FromStatusId = table.Column<int>(type: "int", nullable: false),
                    ToStatusId = table.Column<int>(type: "int", nullable: false),
                    RequestedBy = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RequestedReason = table.Column<string>(type: "varchar(500)", nullable: false),
                    CfoApproverId = table.Column<int>(type: "int", nullable: true),
                    CfoApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SysAdminApproverId = table.Column<int>(type: "int", nullable: true),
                    SysAdminApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    OverrideStatusId = table.Column<int>(type: "int", nullable: false),
                    AppliedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectionReason = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_PeriodStatusOverride", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeriodStatusOverride_AccountingPeriod_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalSchema: "Finance",
                        principalTable: "AccountingPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodStatusOverride_MiscMaster_FromStatusId",
                        column: x => x.FromStatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodStatusOverride_MiscMaster_OverrideStatusId",
                        column: x => x.OverrideStatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PeriodStatusOverride_MiscMaster_ToStatusId",
                        column: x => x.ToStatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeriodStatusOverride_AccountingPeriodId",
                schema: "Finance",
                table: "PeriodStatusOverride",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodStatusOverride_CompanyId",
                schema: "Finance",
                table: "PeriodStatusOverride",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodStatusOverride_FromStatusId",
                schema: "Finance",
                table: "PeriodStatusOverride",
                column: "FromStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodStatusOverride_OverrideStatusId",
                schema: "Finance",
                table: "PeriodStatusOverride",
                column: "OverrideStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodStatusOverride_ToStatusId",
                schema: "Finance",
                table: "PeriodStatusOverride",
                column: "ToStatusId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ─── PeriodStatusOverride ─────────────────────────────────────────────────
            migrationBuilder.DropTable(
                name: "PeriodStatusOverride",
                schema: "Finance");

            // ─── JournalHeader backdate columns + index ───────────────────────────────
            migrationBuilder.DropIndex(
                name: "IX_JournalHeader_IsBackdated",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "BackdateAcknowledgedAt",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "BackdateAcknowledgedBy",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "BackdateReason",
                schema: "Finance",
                table: "JournalHeader");

            migrationBuilder.DropColumn(
                name: "IsBackdated",
                schema: "Finance",
                table: "JournalHeader");

            // ─── AccountingPeriod 3 columns + index ───────────────────────────────────
            migrationBuilder.DropIndex(
                name: "IX_AccountingPeriod_AdjustmentPerFY",
                schema: "Finance",
                table: "AccountingPeriod");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedAt",
                schema: "Finance",
                table: "AccountingPeriod");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedBy",
                schema: "Finance",
                table: "AccountingPeriod");

            migrationBuilder.DropColumn(
                name: "IsAdjustmentPeriod",
                schema: "Finance",
                table: "AccountingPeriod");
        }
    }
}
