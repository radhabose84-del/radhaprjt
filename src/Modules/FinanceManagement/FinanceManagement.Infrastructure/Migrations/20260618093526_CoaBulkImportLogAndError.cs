using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CoaBulkImportLogAndError : Migration
    {
        // GL02-FR-006 — COA bulk import/export. Contains only the COA changes:
        // the GlAccountMaster.ImportLogId traceability column and the two import-audit tables.
        // (The ScheduleIII subtotal rework was already applied by 20260618091928_ScheduleIIISubTotalRework;
        //  it was duplicated here by snapshot drift and has been removed.)

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImportLogId",
                schema: "Finance",
                table: "GlAccountMaster",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GlAccountImportLog",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "varchar(260)", nullable: false),
                    FileFormat = table.Column<string>(type: "varchar(10)", nullable: false),
                    ImportMode = table.Column<string>(type: "varchar(20)", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false),
                    GroupRows = table.Column<int>(type: "int", nullable: false),
                    AccountRows = table.Column<int>(type: "int", nullable: false),
                    ValidRows = table.Column<int>(type: "int", nullable: false),
                    InvalidRows = table.Column<int>(type: "int", nullable: false),
                    ImportedGroups = table.Column<int>(type: "int", nullable: false),
                    ImportedAccounts = table.Column<int>(type: "int", nullable: false),
                    SkippedRows = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", nullable: false),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_GlAccountImportLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlAccountImportError",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportLogId = table.Column<int>(type: "int", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    RecordType = table.Column<string>(type: "varchar(10)", nullable: true),
                    ColumnName = table.Column<string>(type: "varchar(100)", nullable: true),
                    AttemptedValue = table.Column<string>(type: "varchar(200)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "varchar(500)", nullable: false),
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
                    table.PrimaryKey("PK_GlAccountImportError", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlAccountImportError_GlAccountImportLog_ImportLogId",
                        column: x => x.ImportLogId,
                        principalSchema: "Finance",
                        principalTable: "GlAccountImportLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountMaster_ImportLogId",
                schema: "Finance",
                table: "GlAccountMaster",
                column: "ImportLogId");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountImportError_ImportLogId",
                schema: "Finance",
                table: "GlAccountImportError",
                column: "ImportLogId");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountImportLog_CompanyId",
                schema: "Finance",
                table: "GlAccountImportLog",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GlAccountImportLog_CreatedDate",
                schema: "Finance",
                table: "GlAccountImportLog",
                column: "CreatedDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlAccountImportError",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "GlAccountImportLog",
                schema: "Finance");

            migrationBuilder.DropIndex(
                name: "IX_GlAccountMaster_ImportLogId",
                schema: "Finance",
                table: "GlAccountMaster");

            migrationBuilder.DropColumn(
                name: "ImportLogId",
                schema: "Finance",
                table: "GlAccountMaster");
        }
    }
}
