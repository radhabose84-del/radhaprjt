using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PeriodStatusControls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastStatusChangedAt",
                schema: "Finance",
                table: "FinancialPeriodMaster",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastStatusChangedBy",
                schema: "Finance",
                table: "FinancialPeriodMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PeriodStatusOverride",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialPeriodId = table.Column<int>(type: "int", nullable: false),
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
                        name: "FK_PeriodStatusOverride_FinancialPeriodMaster_FinancialPeriodId",
                        column: x => x.FinancialPeriodId,
                        principalSchema: "Finance",
                        principalTable: "FinancialPeriodMaster",
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
                name: "IX_PeriodStatusOverride_CompanyId",
                schema: "Finance",
                table: "PeriodStatusOverride",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodStatusOverride_FinancialPeriodId",
                schema: "Finance",
                table: "PeriodStatusOverride",
                column: "FinancialPeriodId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeriodStatusOverride",
                schema: "Finance");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedAt",
                schema: "Finance",
                table: "FinancialPeriodMaster");

            migrationBuilder.DropColumn(
                name: "LastStatusChangedBy",
                schema: "Finance",
                table: "FinancialPeriodMaster");
        }
    }
}
