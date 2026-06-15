using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleIIIActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLog",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    ScopeKey = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlAccountMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    AccountTypeId = table.Column<int>(type: "int", nullable: false),
                    AccountGroupId = table.Column<int>(type: "int", nullable: false),
                    AccountCode = table.Column<string>(type: "varchar(20)", nullable: false),
                    AccountName = table.Column<string>(type: "varchar(200)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", nullable: true),
                    NormalBalanceId = table.Column<int>(type: "int", nullable: false),
                    CurrencyTypeId = table.Column<int>(type: "int", nullable: false),
                    SubLedgerTypeId = table.Column<int>(type: "int", nullable: false),
                    IsCostCentreMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsTaxRelevant = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsInterCompany = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsReconciliationRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_GlAccountMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlAccountMaster_AccountGroup_AccountGroupId",
                        column: x => x.AccountGroupId,
                        principalSchema: "Finance",
                        principalTable: "AccountGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GlAccountMaster_AccountTypeMaster_AccountTypeId",
                        column: x => x.AccountTypeId,
                        principalSchema: "Finance",
                        principalTable: "AccountTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GlAccountMaster_MiscMaster_NormalBalanceId",
                        column: x => x.NormalBalanceId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GlAccountMaster_MiscMaster_SubLedgerTypeId",
                        column: x => x.SubLedgerTypeId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLog_Entity_CreatedDate",
                schema: "Finance",
                table: "ActivityLog",
                columns: new[] { "EntityName", "EntityId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_AccountGroupId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "AccountGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_AccountTypeId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "AccountTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_CompanyId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_CompanyId_AccountCode",
                schema: "Finance",
                table: "GlAccountMaster",
                columns: new[] { "CompanyId", "AccountCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_CompanyId_AccountName",
                schema: "Finance",
                table: "GlAccountMaster",
                columns: new[] { "CompanyId", "AccountName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_NormalBalanceId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "NormalBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_SubLedgerTypeId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "SubLedgerTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLog",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "GlAccountMaster",
                schema: "Finance");
        }
    }
}
