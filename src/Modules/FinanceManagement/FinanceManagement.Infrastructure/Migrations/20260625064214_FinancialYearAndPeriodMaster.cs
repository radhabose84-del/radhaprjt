using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinancialYearAndPeriodMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialYearMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearCode = table.Column<string>(type: "varchar(9)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    IsTransitionYear = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_FinancialYearMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialYearMaster_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialPeriodMaster",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    PeriodNumber = table.Column<byte>(type: "tinyint", nullable: false),
                    PeriodName = table.Column<string>(type: "varchar(20)", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    IsAdjustmentPeriod = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_FinancialPeriodMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialPeriodMaster_FinancialYearMaster_FinancialYearId",
                        column: x => x.FinancialYearId,
                        principalSchema: "Finance",
                        principalTable: "FinancialYearMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FinancialPeriodMaster_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPeriodMaster_CompanyId",
                schema: "Finance",
                table: "FinancialPeriodMaster",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPeriodMaster_CompanyId_StartDate_EndDate_IsAdjustmentPeriod",
                schema: "Finance",
                table: "FinancialPeriodMaster",
                columns: new[] { "CompanyId", "StartDate", "EndDate", "IsAdjustmentPeriod" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPeriodMaster_FinancialYearId_PeriodNumber",
                schema: "Finance",
                table: "FinancialPeriodMaster",
                columns: new[] { "FinancialYearId", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPeriodMaster_StartDate_EndDate",
                schema: "Finance",
                table: "FinancialPeriodMaster",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialPeriodMaster_StatusId",
                schema: "Finance",
                table: "FinancialPeriodMaster",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialYearMaster_CompanyId",
                schema: "Finance",
                table: "FinancialYearMaster",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialYearMaster_CompanyId_FinancialYearCode",
                schema: "Finance",
                table: "FinancialYearMaster",
                columns: new[] { "CompanyId", "FinancialYearCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialYearMaster_CompanyId_StartDate_EndDate",
                schema: "Finance",
                table: "FinancialYearMaster",
                columns: new[] { "CompanyId", "StartDate", "EndDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialYearMaster_EndDate",
                schema: "Finance",
                table: "FinancialYearMaster",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialYearMaster_StatusId",
                schema: "Finance",
                table: "FinancialYearMaster",
                column: "StatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialPeriodMaster",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "FinancialYearMaster",
                schema: "Finance");
        }
    }
}
