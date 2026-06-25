using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Application.Common.Interfaces.AuditLog;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IDocumentSequence;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Application.Common.Interfaces.IMiscTypeMaster;
using FinanceManagement.Application.Common.Interfaces.IMiscMaster;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ISecurityViolationLog;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalFlag;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringGeneration;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IGapScan;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Services;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Services;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.AccountingPeriod;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.Journal;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.RecurringJournalTemplate;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalThresholdRule;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.JournalImport;
using FinanceManagement.Infrastructure.Repositories.JournalMaster.SecurityViolationLog;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.Common.Interfaces.ICoaReport;
using FinanceManagement.Infrastructure.Repositories.CoaReport;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;
using FinanceManagement.Infrastructure.Repositories.CoaFreeze;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Infrastructure.Repositories.CoaChangeRequest;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Infrastructure.Repositories.FinancialYearMaster;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Infrastructure.Repositories.PeriodStatusOverride;
using FinanceManagement.Application.Common.Interfaces.ICostCentre;
using FinanceManagement.Application.Common.Interfaces.IProfitCentre;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Persistence;
using FinanceManagement.Infrastructure.Repositories.AuditLog;
using Contracts.Interfaces.Lookups.Finance;
using FinanceManagement.Infrastructure.Repositories.AccountGroup;
using FinanceManagement.Infrastructure.Repositories.DocumentSequence;
using FinanceManagement.Infrastructure.Repositories.EInvoiceHeader;
using FinanceManagement.Infrastructure.Repositories.EWaybillHeader;
using FinanceManagement.Infrastructure.Repositories.Lookups.Finance;
using FinanceManagement.Infrastructure.Repositories.TransactionTypeMaster;
using FinanceManagement.Infrastructure.Repositories.MiscTypeMaster;
using FinanceManagement.Infrastructure.Repositories.MiscMaster;
using FinanceManagement.Infrastructure.Repositories.AccountTypeMaster;
using FinanceManagement.Infrastructure.Repositories.VoucherType;

using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Infrastructure.Repositories.ScheduleIII;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Infrastructure.Repositories.TaxCode;
using FinanceManagement.Infrastructure.Logging;
using FinanceManagement.Infrastructure.Repositories.GlAccountMaster;
using FinanceManagement.Application.Common.Interfaces.IGlAccountImport;
using FinanceManagement.Application.GlAccountImport.Services;
using FinanceManagement.Infrastructure.Repositories.GlAccountImport;
using FinanceManagement.Infrastructure.Repositories.CurrencyForexConfig;
using FinanceManagement.Infrastructure.Repositories.CostCentre;
using FinanceManagement.Infrastructure.Repositories.ProfitCentre;
using FinanceManagement.Infrastructure.Repositories.Outbox;
using FinanceManagement.Infrastructure.Services;
using FinanceManagement.Infrastructure.Services.Outbox;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Shared.Infrastructure.Resilience;
using Serilog;

namespace FinanceManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddFinanceInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            var connectionString = (configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
                .Replace("{SERVER}", Environment.GetEnvironmentVariable("DATABASE_SERVER") ?? "")
                .Replace("{USER_ID}", Environment.GetEnvironmentVariable("DATABASE_USERID") ?? "")
                .Replace("{ENC_PASSWORD}", Environment.GetEnvironmentVariable("DATABASE_PASSWORD") ?? "");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found or is empty.");

            // US-GL02-08B — configurable CFO/SysAdmin/FC/Internal-Audit role mapping + unfreeze window.
            services.Configure<FinanceManagement.Application.Common.Options.CoaUnfreezeOptions>(
                configuration.GetSection(FinanceManagement.Application.Common.Options.CoaUnfreezeOptions.SectionName));

            // US-GL02-10 — multi-company COA template company (binds "MultiCompanyCoa"; 0 if unset).
            services.Configure<FinanceManagement.Application.Common.Options.MultiCompanyCoaOptions>(
                configuration.GetSection(FinanceManagement.Application.Common.Options.MultiCompanyCoaOptions.SectionName));

            // Activity-log interceptor (writes Finance.ActivityLog for IActivityTracked entities)
            services.AddScoped<ActivityLogSaveChangesInterceptor>();

            // US-GL02-09 — statutory audit-trail interceptor (writes Finance.AccountAuditTrail for IAuditTrailed entities)
            services.AddScoped<AccountAuditTrailSaveChangesInterceptor>();

            // Register ApplicationDbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
                .AddInterceptors(sp.GetRequiredService<ActivityLogSaveChangesInterceptor>())
                .AddInterceptors(sp.GetRequiredService<AccountAuditTrailSaveChangesInterceptor>()));

            // Register IDbConnection for Dapper
            services.AddTransient<IDbConnection>(sp => new SqlConnection(connectionString));

            // MongoDB Context
            services.AddSingleton<IMongoClient>(sp =>
            {
                var mongoConnectionString = configuration.GetConnectionString("MongoDbConnectionString");
                if (string.IsNullOrWhiteSpace(mongoConnectionString))
                    throw new InvalidOperationException("MongoDB connection string is missing or empty.");
                return new MongoClient(mongoConnectionString);
            });

            services.AddSingleton<IMongoDbContext>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var databaseName = configuration["MongoDb:DatabaseName"];
                if (string.IsNullOrWhiteSpace(databaseName))
                    throw new InvalidOperationException("MongoDB database name is missing or empty.");
                return new MongoDbContext(client, databaseName);
            });

            services.AddSingleton(sp =>
            {
                var mongoDbContext = (MongoDbContext)sp.GetRequiredService<IMongoDbContext>();
                return mongoDbContext.GetDatabase();
            });

            services.AddSingleton<IMongoCollection<OutboxMessage>>(sp =>
            {
                var db = sp.GetRequiredService<IMongoDatabase>();
                return db.GetCollection<OutboxMessage>("OutboxMessages");
            });

            services.AddLogging(builder => builder.AddSerilog());

            services.AddHttpContextAccessor();
            services.AddTransient<AuthTokenHandler>();

            // ── Core Services ────────────────────────────────────────────────
            services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            services.AddTransient<IFileUploadService, FileUploadRepository>();
            services.AddSingleton<ITimeZoneService, TimeZoneService>();
            services.AddTransient<IJwtTokenHelper, JwtTokenHelper>();
            services.AddScoped<ILogQueryService, LogQueryService>();

            // ── Entity repositories ──────────────────────────────────────────
            services.AddScoped<ITransactionTypeMasterCommandRepository, TransactionTypeMasterCommandRepository>();
            services.AddScoped<ITransactionTypeMasterQueryRepository, TransactionTypeMasterQueryRepository>();

            services.AddScoped<IDocumentSequenceCommandRepository, DocumentSequenceCommandRepository>();
            services.AddScoped<IDocumentSequenceQueryRepository, DocumentSequenceQueryRepository>();

            services.AddScoped<IAccountGroupCommandRepository, AccountGroupCommandRepository>();
            services.AddScoped<IAccountGroupQueryRepository, AccountGroupQueryRepository>();
            services.AddScoped<IAccountGroupChangeRequestRepository, AccountGroupChangeRequestRepository>();

            // Transactional outbox (workflow events → bus)
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();

            services.AddScoped<IEInvoiceHeaderCommandRepository, EInvoiceHeaderCommandRepository>();
            services.AddScoped<IEInvoiceHeaderQueryRepository, EInvoiceHeaderQueryRepository>();

            services.AddScoped<IEWaybillHeaderCommandRepository, EWaybillHeaderCommandRepository>();
            services.AddScoped<IEWaybillHeaderQueryRepository, EWaybillHeaderQueryRepository>();

            services.AddScoped<IMiscTypeMasterCommandRepository, MiscTypeMasterCommandRepository>();
            services.AddScoped<IMiscTypeMasterQueryRepository, MiscTypeMasterQueryRepository>();

            services.AddScoped<IMiscMasterCommandRepository, MiscMasterCommandRepository>();
            services.AddScoped<IMiscMasterQueryRepository, MiscMasterQueryRepository>();

            services.AddScoped<IAccountTypeMasterCommandRepository, AccountTypeMasterCommandRepository>();
            services.AddScoped<IAccountTypeMasterQueryRepository, AccountTypeMasterQueryRepository>();

            // Voucher Type configuration master (US-GL01-02)
            services.AddScoped<IVoucherTypeMasterCommandRepository, VoucherTypeMasterCommandRepository>();
            services.AddScoped<IVoucherTypeMasterQueryRepository, VoucherTypeMasterQueryRepository>();

            // Accounting Period master (GL-03 / JournalMaster GL01)
            services.AddScoped<IAccountingPeriodCommandRepository, AccountingPeriodCommandRepository>();
            services.AddScoped<IAccountingPeriodQueryRepository, AccountingPeriodQueryRepository>();

            // Journal voucher entry aggregate (JournalHeader + JournalDetail) — US-GL01-01/05
            services.AddScoped<IJournalCommandRepository, JournalCommandRepository>();
            services.AddScoped<IJournalQueryRepository, JournalQueryRepository>();

            // Ledger balance read (period balances + GL/account-type/account-group info)
            services.AddScoped<FinanceManagement.Application.Common.Interfaces.JournalMaster.ILedgerBalance.ILedgerBalanceQueryRepository,
                FinanceManagement.Infrastructure.Repositories.JournalMaster.LedgerBalances.LedgerBalanceQueryRepository>();

            // Recurring journal template authoring (Header + Detail) — US-GL01-11A
            services.AddScoped<IRecurringJournalTemplateCommandRepository, RecurringJournalTemplateCommandRepository>();
            services.AddScoped<IRecurringJournalTemplateQueryRepository, RecurringJournalTemplateQueryRepository>();

            // Journal threshold rules (US-GL01-16A) + flag read (US-GL01-16B)
            services.AddScoped<IJournalThresholdRuleCommandRepository, JournalThresholdRuleCommandRepository>();
            services.AddScoped<IJournalThresholdRuleQueryRepository, JournalThresholdRuleQueryRepository>();

            // Flagging engine (US-GL01-16B) — evaluates thresholds on posting + raises JournalFlag rows
            services.AddScoped<IJournalFlagEngineRepository, JournalFlagEngineRepository>();

            // Recurring generation (US-GL01-11B) — instantiate due templates on period-open
            services.AddScoped<IRecurringGenerationRepository, RecurringGenerationRepository>();
            services.AddScoped<IRecurringJournalGenerationService, RecurringJournalGenerationService>();

            // Sequence gap detection (US-GL01-03B) — scan number series for missing numbers
            services.AddScoped<IGapScanRepository, GapScanRepository>();
            services.AddScoped<IGapScanService, GapScanService>();

            // Bulk journal import from Excel (US-GL01-17)
            services.AddScoped<IJournalImportCommandRepository, JournalImportCommandRepository>();
            services.AddScoped<IJournalImportQueryRepository, JournalImportQueryRepository>();
            services.AddScoped<IJournalImportFileService, FinanceManagement.Application.JournalMaster.JournalImport.Services.JournalImportFileService>();

            // Posted-journal immutability — security violation log read (US-GL01-10; rows written by DB triggers)
            services.AddScoped<ISecurityViolationLogQueryRepository, SecurityViolationLogQueryRepository>();

            services.AddScoped<IGlAccountMasterCommandRepository, GlAccountMasterCommandRepository>();
            services.AddScoped<IGlAccountMasterQueryRepository, GlAccountMasterQueryRepository>();

            // US-GL03-01 — Financial Year + auto-generated 13 periods
            // (Hangfire job lives in BackgroundService.Infrastructure.Jobs — registered there.)
            services.AddScoped<IFinancialYearMasterCommandRepository, FinancialYearMasterCommandRepository>();
            services.AddScoped<IFinancialYearMasterQueryRepository, FinancialYearMasterQueryRepository>();

            // US-GL02-10 — multi-company COA inheritance + propagation of the global template.
            services.AddScoped<IGlobalCoaPropagationService, GlobalCoaPropagationService>();

            // US-GL02-15 — COA listing & structure reports (read-only) + QuestPDF listing export.
            // QuestPDF Community license is set once here (free under the Community terms).
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            services.AddScoped<ICoaReportQueryRepository, CoaReportQueryRepository>();
            services.AddScoped<ICoaListingPdfBuilder, CoaListingPdfBuilder>();

            // US-GL03-02 — Period status state machine + dual-approval reversal flow
            services.AddScoped<IPeriodStatusOverrideCommandRepository, PeriodStatusOverrideCommandRepository>();
            services.AddScoped<IPeriodStatusOverrideQueryRepository, PeriodStatusOverrideQueryRepository>();
            services.AddScoped<IPeriodPostingGate, FinanceManagement.Infrastructure.Services.PeriodPostingGate>();

            // Account type-ahead per-user favourites + recently-used (US-GL02-07) — SQL tables
            // (Finance.GlAccountFavourite / GlAccountRecentUse), FK to GlAccountMaster.
            services.AddScoped<IGlAccountUserPrefStore, GlAccountUserPrefRepository>();

            // US-GL02-09 — statutory account audit trail (read-only viewer + export).
            services.AddScoped<FinanceManagement.Application.Common.Interfaces.IAccountAuditTrail.IAccountAuditTrailQueryRepository,
                FinanceManagement.Infrastructure.Repositories.AccountAuditTrail.AccountAuditTrailQueryRepository>();

            // COA Freeze engine (US-GL02-FR-008a) — state read/write + Mongo violation log.
            services.AddScoped<ICoaFreezeQueryRepository, CoaFreezeQueryRepository>();
            services.AddScoped<ICoaFreezeCommandRepository, CoaFreezeCommandRepository>();
            services.AddScoped<ICoaFreezeViolationLog, CoaFreezeViolationLogStore>();

            // COA change-request + dual-approval unfreeze workflow (US-GL02-08B).
            services.AddScoped<ICoaChangeRequestCommandRepository, CoaChangeRequestCommandRepository>();
            services.AddScoped<ICoaChangeRequestQueryRepository, CoaChangeRequestQueryRepository>();

            // COA bulk import/export (GL02-FR-006)
            services.AddScoped<IGlAccountImportCommandRepository, GlAccountImportCommandRepository>();
            services.AddScoped<IGlAccountImportQueryRepository, GlAccountImportQueryRepository>();
            services.AddScoped<IGlAccountImportFileService, GlAccountImportFileService>();
            services.AddScoped<IGlAccountImportValidator, GlAccountImportValidator>();

            services.AddScoped<ICurrencyForexConfigCommandRepository, CurrencyForexConfigCommandRepository>();
            services.AddScoped<ICurrencyForexConfigQueryRepository, CurrencyForexConfigQueryRepository>();

            // Cost Centre master & 3-level hierarchy (US-GL05-01)
            services.AddScoped<ICostCentreCommandRepository, CostCentreCommandRepository>();
            services.AddScoped<ICostCentreQueryRepository, CostCentreQueryRepository>();

            // Profit Centre master & 2-level hierarchy (US-GL05-02)
            services.AddScoped<IProfitCentreCommandRepository, ProfitCentreCommandRepository>();
            services.AddScoped<IProfitCentreQueryRepository, ProfitCentreQueryRepository>();

            // Tax Code feature (US-GL02-05A / 05B) — consolidated command + query repos
            services.AddScoped<ITaxCodeCommandRepository, TaxCodeCommandRepository>();
            services.AddScoped<ITaxCodeQueryRepository, TaxCodeQueryRepository>();

            // GSTR section master + account-range mapping — consolidated command + query repos
            services.AddScoped<IGstrSectionCommandRepository, GstrSectionCommandRepository>();
            services.AddScoped<IGstrSectionQueryRepository, GstrSectionQueryRepository>();

            // ── Lookup repositories (consumed by other modules via Contracts) ──
            services.AddScoped<IDocumentSequenceLookup, DocumentSequenceLookupRepository>();
            services.AddScoped<ITransactionTypeLookup, TransactionTypeLookupRepository>();
            services.AddScoped<IEInvoiceLookup, EInvoiceLookupRepository>();
            services.AddScoped<IEWaybillLookup, EWaybillLookupRepository>();
            services.AddScoped<IAccountTypeMasterLookup, AccountTypeMasterLookupRepository>();
           services.AddScoped<IScheduleIIICommandRepository, ScheduleIIICommandRepository>();
            services.AddScoped<IScheduleIIIQueryRepository, ScheduleIIIQueryRepository>();
            services.AddScoped<IGlAccountMasterLookup, GlAccountMasterLookupRepository>();
            services.AddScoped<ITaxCodeLookup, TaxCodeLookupRepository>();
            services.AddScoped<IFinancialPeriodMasterLookup, FinancialPeriodMasterLookupRepository>();

            // ── NIC E-Invoice service ─────────────────────────────────────────
            // Named HttpClient for NIC API calls; base address is set dynamically
            // inside NicEInvoiceService.GetConfig() so the client has no base address here.
            // SSL validation is intentionally bypassed for the NIC sandbox which uses
            // a self-signed certificate.
            services.AddHttpClient("NicEInvoice")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                })
                .AddBsoftHttpResilience(ResilienceProfileNames.Critical);

            services.AddScoped<INicEInvoiceService, NicEInvoiceService>();

            // Validation repositories — cross-module referential integrity (Rule 25)
            services.AddScoped<Contracts.Interfaces.Validations.FinanceManagement.IPartyMasterFinanceValidation, Repositories.Validations.PartyMasterFinanceValidationRepository>();

            return services;
        }
    }
}
