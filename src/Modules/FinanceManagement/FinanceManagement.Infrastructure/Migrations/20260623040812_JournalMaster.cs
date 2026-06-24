using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class JournalMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountingPeriod",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    PeriodName = table.Column<string>(type: "varchar(20)", nullable: false),
                    PeriodNo = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AccountingPeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountingPeriod_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalImportBatch",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "varchar(260)", nullable: false),
                    TotalRows = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ValidRows = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ErrorRows = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    ImportedBy = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_JournalImportBatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalImportBatch_MiscMaster_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalImportBatch_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalSavedFilter",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", nullable: false),
                    CriteriaJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalSavedFilter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JournalThresholdRule",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleTypeId = table.Column<int>(type: "int", nullable: false),
                    ThresholdValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
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
                    table.PrimaryKey("PK_JournalThresholdRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalThresholdRule_MiscMaster_RuleTypeId",
                        column: x => x.RuleTypeId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecurringJournalTemplateHeader",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateName = table.Column<string>(type: "varchar(150)", nullable: false),
                    VoucherTypeId = table.Column<int>(type: "int", nullable: false),
                    FrequencyId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    AutoPost = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AmountAdjustmentRuleId = table.Column<int>(type: "int", nullable: false),
                    LowRisk = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_RecurringJournalTemplateHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringJournalTemplateHeader_MiscMaster_AmountAdjustmentRuleId",
                        column: x => x.AmountAdjustmentRuleId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringJournalTemplateHeader_MiscMaster_FrequencyId",
                        column: x => x.FrequencyId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringJournalTemplateHeader_VoucherTypeMaster_VoucherTypeId",
                        column: x => x.VoucherTypeId,
                        principalSchema: "Finance",
                        principalTable: "VoucherTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SecurityViolationLog",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "varchar(50)", nullable: false),
                    JournalHeaderId = table.Column<int>(type: "int", nullable: true),
                    AttemptedAction = table.Column<string>(type: "varchar(10)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(100)", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Channel = table.Column<string>(type: "varchar(30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityViolationLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SequenceGapScanLog",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SeriesId = table.Column<int>(type: "int", nullable: false),
                    ScannedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GapsFound = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    GapNumbers = table.Column<string>(type: "varchar(max)", nullable: true),
                    Alerted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SequenceGapScanLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SequenceGapScanLog_VoucherTypeNumberSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalSchema: "Finance",
                        principalTable: "VoucherTypeNumberSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LedgerBalance",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    GlAccountId = table.Column<int>(type: "int", nullable: false),
                    AccountingPeriodId = table.Column<int>(type: "int", nullable: false),
                    CostCentreId = table.Column<int>(type: "int", nullable: true),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    DrTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    CrTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    CostCentreKey = table.Column<int>(type: "int", nullable: false, computedColumnSql: "ISNULL([CostCentreId], 0)", stored: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerBalance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LedgerBalance_AccountingPeriod_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalSchema: "Finance",
                        principalTable: "AccountingPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerBalance_CostCentre_CostCentreId",
                        column: x => x.CostCentreId,
                        principalSchema: "Finance",
                        principalTable: "CostCentre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerBalance_GlAccountMaster_GlAccountId",
                        column: x => x.GlAccountId,
                        principalSchema: "Finance",
                        principalTable: "GlAccountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalHeader",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: true),
                    VoucherTypeId = table.Column<int>(type: "int", nullable: false),
                    VoucherNo = table.Column<string>(type: "varchar(30)", nullable: true),
                    VoucherDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PostingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    FinancialYearId = table.Column<int>(type: "int", nullable: false),
                    AccountingPeriodId = table.Column<int>(type: "int", nullable: true),
                    Narration = table.Column<string>(type: "varchar(500)", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    SourceId = table.Column<int>(type: "int", nullable: false),
                    TriggerDocType = table.Column<string>(type: "varchar(30)", nullable: true),
                    TriggerDocRef = table.Column<string>(type: "varchar(50)", nullable: true),
                    AutoApproved = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TotalDr = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    TotalCr = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    ReversalOfId = table.Column<int>(type: "int", nullable: true),
                    IsReversal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CopiedFromRef = table.Column<string>(type: "varchar(30)", nullable: true),
                    ImportBatchId = table.Column<int>(type: "int", nullable: true),
                    DraftSavedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CleanupAlertedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    SubmittedBy = table.Column<int>(type: "int", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectedBy = table.Column<int>(type: "int", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RejectReason = table.Column<string>(type: "varchar(500)", nullable: true),
                    PostedBy = table.Column<int>(type: "int", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
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
                    table.PrimaryKey("PK_JournalHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalHeader_AccountingPeriod_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalSchema: "Finance",
                        principalTable: "AccountingPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalHeader_JournalHeader_ReversalOfId",
                        column: x => x.ReversalOfId,
                        principalSchema: "Finance",
                        principalTable: "JournalHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalHeader_JournalImportBatch_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalSchema: "Finance",
                        principalTable: "JournalImportBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalHeader_MiscMaster_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalHeader_MiscMaster_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalHeader_VoucherTypeMaster_VoucherTypeId",
                        column: x => x.VoucherTypeId,
                        principalSchema: "Finance",
                        principalTable: "VoucherTypeMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalImportError",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImportBatchId = table.Column<int>(type: "int", nullable: false),
                    RowNo = table.Column<int>(type: "int", nullable: false),
                    ColumnName = table.Column<string>(type: "varchar(50)", nullable: true),
                    Message = table.Column<string>(type: "varchar(500)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalImportError", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalImportError_JournalImportBatch_ImportBatchId",
                        column: x => x.ImportBatchId,
                        principalSchema: "Finance",
                        principalTable: "JournalImportBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecurringJournalTemplateDetail",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    GlAccountId = table.Column<int>(type: "int", nullable: false),
                    DrAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CrAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AmountFormula = table.Column<string>(type: "varchar(300)", nullable: true),
                    CostCentreId = table.Column<int>(type: "int", nullable: true),
                    ProfitCentreId = table.Column<int>(type: "int", nullable: true),
                    LineNarration = table.Column<string>(type: "varchar(500)", nullable: true),
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
                    table.PrimaryKey("PK_RecurringJournalTemplateDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringJournalTemplateDetail_CostCentre_CostCentreId",
                        column: x => x.CostCentreId,
                        principalSchema: "Finance",
                        principalTable: "CostCentre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringJournalTemplateDetail_GlAccountMaster_GlAccountId",
                        column: x => x.GlAccountId,
                        principalSchema: "Finance",
                        principalTable: "GlAccountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringJournalTemplateDetail_ProfitCentre_ProfitCentreId",
                        column: x => x.ProfitCentreId,
                        principalSchema: "Finance",
                        principalTable: "ProfitCentre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringJournalTemplateDetail_RecurringJournalTemplateHeader_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "Finance",
                        principalTable: "RecurringJournalTemplateHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalDetail",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalHeaderId = table.Column<int>(type: "int", nullable: false),
                    LineNo = table.Column<int>(type: "int", nullable: false),
                    GlAccountId = table.Column<int>(type: "int", nullable: false),
                    DrAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    CrAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                    BaseDrAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BaseCrAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CostCentreId = table.Column<int>(type: "int", nullable: true),
                    ProfitCentreId = table.Column<int>(type: "int", nullable: true),
                    LineNarration = table.Column<string>(type: "varchar(500)", nullable: true),
                    ReferenceDocNo = table.Column<string>(type: "varchar(50)", nullable: true),
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
                    table.PrimaryKey("PK_JournalDetail", x => x.Id);
                    table.CheckConstraint("CK_JournalDetail_DrCr", "[DrAmount] = 0 OR [CrAmount] = 0");
                    table.ForeignKey(
                        name: "FK_JournalDetail_CostCentre_CostCentreId",
                        column: x => x.CostCentreId,
                        principalSchema: "Finance",
                        principalTable: "CostCentre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalDetail_CurrencyForexConfig_CurrencyId",
                        column: x => x.CurrencyId,
                        principalSchema: "Finance",
                        principalTable: "CurrencyForexConfig",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalDetail_GlAccountMaster_GlAccountId",
                        column: x => x.GlAccountId,
                        principalSchema: "Finance",
                        principalTable: "GlAccountMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalDetail_JournalHeader_JournalHeaderId",
                        column: x => x.JournalHeaderId,
                        principalSchema: "Finance",
                        principalTable: "JournalHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalDetail_ProfitCentre_ProfitCentreId",
                        column: x => x.ProfitCentreId,
                        principalSchema: "Finance",
                        principalTable: "ProfitCentre",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JournalFlag",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalHeaderId = table.Column<int>(type: "int", nullable: false),
                    RuleTypeId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FlaggedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    DigestSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalFlag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalFlag_JournalHeader_JournalHeaderId",
                        column: x => x.JournalHeaderId,
                        principalSchema: "Finance",
                        principalTable: "JournalHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalFlag_MiscMaster_RuleTypeId",
                        column: x => x.RuleTypeId,
                        principalSchema: "Finance",
                        principalTable: "MiscMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RecurringGenerationLog",
                schema: "Finance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "varchar(20)", nullable: false),
                    GeneratedVoucherId = table.Column<int>(type: "int", nullable: true),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    AutoPosted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringGenerationLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringGenerationLog_JournalHeader_GeneratedVoucherId",
                        column: x => x.GeneratedVoucherId,
                        principalSchema: "Finance",
                        principalTable: "JournalHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecurringGenerationLog_RecurringJournalTemplateHeader_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "Finance",
                        principalTable: "RecurringJournalTemplateHeader",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriod_CompanyId_FinancialYearId",
                schema: "Finance",
                table: "AccountingPeriod",
                columns: new[] { "CompanyId", "FinancialYearId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriod_CompanyId_FinancialYearId_PeriodNo",
                schema: "Finance",
                table: "AccountingPeriod",
                columns: new[] { "CompanyId", "FinancialYearId", "PeriodNo" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriod_StartDate_EndDate",
                schema: "Finance",
                table: "AccountingPeriod",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountingPeriod_StatusId",
                schema: "Finance",
                table: "AccountingPeriod",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalDetail_CostCentreId",
                schema: "Finance",
                table: "JournalDetail",
                column: "CostCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalDetail_CurrencyId",
                schema: "Finance",
                table: "JournalDetail",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalDetail_GlAccountId",
                schema: "Finance",
                table: "JournalDetail",
                column: "GlAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalDetail_JournalHeaderId",
                schema: "Finance",
                table: "JournalDetail",
                column: "JournalHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalDetail_ProfitCentreId",
                schema: "Finance",
                table: "JournalDetail",
                column: "ProfitCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalDetail_ReferenceDocNo",
                schema: "Finance",
                table: "JournalDetail",
                column: "ReferenceDocNo");

            migrationBuilder.CreateIndex(
                name: "IX_JournalFlag_FlaggedAt",
                schema: "Finance",
                table: "JournalFlag",
                column: "FlaggedAt");

            migrationBuilder.CreateIndex(
                name: "IX_JournalFlag_JournalHeaderId",
                schema: "Finance",
                table: "JournalFlag",
                column: "JournalHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalFlag_RuleTypeId",
                schema: "Finance",
                table: "JournalFlag",
                column: "RuleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_AccountingPeriodId",
                schema: "Finance",
                table: "JournalHeader",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_CompanyId_VoucherDate",
                schema: "Finance",
                table: "JournalHeader",
                columns: new[] { "CompanyId", "VoucherDate" });

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_CreatedBy",
                schema: "Finance",
                table: "JournalHeader",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_ImportBatchId",
                schema: "Finance",
                table: "JournalHeader",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_ReversalOfId",
                schema: "Finance",
                table: "JournalHeader",
                column: "ReversalOfId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_SourceId",
                schema: "Finance",
                table: "JournalHeader",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_StatusId",
                schema: "Finance",
                table: "JournalHeader",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalHeader_VoucherTypeId",
                schema: "Finance",
                table: "JournalHeader",
                column: "VoucherTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_JournalHeader_VoucherNo",
                schema: "Finance",
                table: "JournalHeader",
                column: "VoucherNo",
                unique: true,
                filter: "[VoucherNo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JournalImportBatch_ImportedBy",
                schema: "Finance",
                table: "JournalImportBatch",
                column: "ImportedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JournalImportBatch_SourceId",
                schema: "Finance",
                table: "JournalImportBatch",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalImportBatch_StatusId",
                schema: "Finance",
                table: "JournalImportBatch",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalImportError_ImportBatchId",
                schema: "Finance",
                table: "JournalImportError",
                column: "ImportBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalSavedFilter_UserId",
                schema: "Finance",
                table: "JournalSavedFilter",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UX_JournalSavedFilter_UserName",
                schema: "Finance",
                table: "JournalSavedFilter",
                columns: new[] { "UserId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalThresholdRule_RuleTypeId",
                schema: "Finance",
                table: "JournalThresholdRule",
                column: "RuleTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerBalance_AccountingPeriodId",
                schema: "Finance",
                table: "LedgerBalance",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerBalance_CostCentreId",
                schema: "Finance",
                table: "LedgerBalance",
                column: "CostCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerBalance_FinancialYearId",
                schema: "Finance",
                table: "LedgerBalance",
                column: "FinancialYearId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerBalance_GlAccountId",
                schema: "Finance",
                table: "LedgerBalance",
                column: "GlAccountId");

            migrationBuilder.CreateIndex(
                name: "UX_LedgerBalance",
                schema: "Finance",
                table: "LedgerBalance",
                columns: new[] { "CompanyId", "GlAccountId", "AccountingPeriodId", "CostCentreKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringGenerationLog_GeneratedAt",
                schema: "Finance",
                table: "RecurringGenerationLog",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringGenerationLog_GeneratedVoucherId",
                schema: "Finance",
                table: "RecurringGenerationLog",
                column: "GeneratedVoucherId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringGenerationLog_TemplateId",
                schema: "Finance",
                table: "RecurringGenerationLog",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "UX_RecurringGenerationLog_CompanyTemplatePeriod",
                schema: "Finance",
                table: "RecurringGenerationLog",
                columns: new[] { "CompanyId", "TemplateId", "Period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecurringJournalTemplateDetail_CostCentreId",
                schema: "Finance",
                table: "RecurringJournalTemplateDetail",
                column: "CostCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringJournalTemplateDetail_GlAccountId",
                schema: "Finance",
                table: "RecurringJournalTemplateDetail",
                column: "GlAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringJournalTemplateDetail_ProfitCentreId",
                schema: "Finance",
                table: "RecurringJournalTemplateDetail",
                column: "ProfitCentreId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringJournalTemplateDetail_TemplateId",
                schema: "Finance",
                table: "RecurringJournalTemplateDetail",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringJournalTemplateHeader_AmountAdjustmentRuleId",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader",
                column: "AmountAdjustmentRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringJournalTemplateHeader_FrequencyId",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader",
                column: "FrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringJournalTemplateHeader_TemplateName",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader",
                column: "TemplateName");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringJournalTemplateHeader_VoucherTypeId",
                schema: "Finance",
                table: "RecurringJournalTemplateHeader",
                column: "VoucherTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityViolationLog_AttemptedAt",
                schema: "Finance",
                table: "SecurityViolationLog",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityViolationLog_JournalHeaderId",
                schema: "Finance",
                table: "SecurityViolationLog",
                column: "JournalHeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceGapScanLog_ScannedAt",
                schema: "Finance",
                table: "SequenceGapScanLog",
                column: "ScannedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SequenceGapScanLog_SeriesId",
                schema: "Finance",
                table: "SequenceGapScanLog",
                column: "SeriesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JournalDetail",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "JournalFlag",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "JournalImportError",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "JournalSavedFilter",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "JournalThresholdRule",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "LedgerBalance",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "RecurringGenerationLog",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "RecurringJournalTemplateDetail",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "SecurityViolationLog",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "SequenceGapScanLog",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "JournalHeader",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "RecurringJournalTemplateHeader",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "AccountingPeriod",
                schema: "Finance");

            migrationBuilder.DropTable(
                name: "JournalImportBatch",
                schema: "Finance");
        }
    }
}
