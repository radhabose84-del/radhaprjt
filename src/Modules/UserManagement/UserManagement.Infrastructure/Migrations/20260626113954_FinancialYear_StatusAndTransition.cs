using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BSOFT.Infrastructure.Migrations
{
    /// <summary>
    /// US-GL03-01..05 refactor (2026-06-26) — adds the FY lifecycle status + transition-year flag to
    /// AppData.FinancialYear so the new period model can scope per-FY status (Open/Closed) without a
    /// separate FinancialYearMaster table in Finance.
    ///
    /// NOTE 1: The auto-generated migration also tried to apply several unrelated operations
    /// (RoleMenuPrivilege audit columns, AccessPolicy/RoleAccessPolicy tables, MiscMaster /
    /// MiscTypeMaster column renames + type changes, IconMaster filtered-index recreation). Those
    /// changes appear because the UserManagement Designer snapshot chain is out of sync with the
    /// current DB schema (same regression as on the Finance side). They are NOT in scope for this
    /// story and almost certainly already exist in the DB from earlier work in another branch.
    ///
    /// NOTE 2: The FK FK_FinancialYear_MiscMaster_StatusId is intentionally OMITTED from this
    /// migration. Existing FinancialYear rows have StatusId = 0 (the column default), which would
    /// violate the FK against AppData.MiscMaster on apply (Id = 0 does not exist). The follow-up
    /// SQL seed (Step 5) inserts the FYS / OPEN MiscMaster row, then runs
    ///     UPDATE AppData.FinancialYear SET StatusId = &lt;new FYS-OPEN id&gt;;
    /// after which the FK is created manually. The Designer + Snapshot still declare the FK so EF
    /// stays happy at compile time and future migrations diff correctly.
    /// </summary>
    public partial class FinancialYear_StatusAndTransition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                schema: "AppData",
                table: "FinancialYear",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTransitionYear",
                schema: "AppData",
                table: "FinancialYear",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialYear_StatusId",
                schema: "AppData",
                table: "FinancialYear",
                column: "StatusId");

            // FK intentionally omitted — see NOTE 2 in the class summary. Added manually in Step 5.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down only reverses what Up did (no FK to drop here).
            migrationBuilder.DropIndex(
                name: "IX_FinancialYear_StatusId",
                schema: "AppData",
                table: "FinancialYear");

            migrationBuilder.DropColumn(
                name: "IsTransitionYear",
                schema: "AppData",
                table: "FinancialYear");

            migrationBuilder.DropColumn(
                name: "StatusId",
                schema: "AppData",
                table: "FinancialYear");
        }
    }
}
