using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IIPAddressService ipAddressService,
            ITimeZoneService timeZoneService)
            : base(options)
        {
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
        }

        public DbSet<TransactionTypeMaster> TransactionTypeMaster => Set<TransactionTypeMaster>();
        public DbSet<DocumentSequence> DocumentSequence => Set<DocumentSequence>();
        public DbSet<AccountGroup> AccountGroup => Set<AccountGroup>();
        public DbSet<AccountGroupChangeRequest> AccountGroupChangeRequest => Set<AccountGroupChangeRequest>();
        public DbSet<EInvoiceHeader> EInvoiceHeader => Set<EInvoiceHeader>();
        public DbSet<EInvoiceDetail> EInvoiceDetail => Set<EInvoiceDetail>();
        public DbSet<EWaybillHeader> EWaybillHeader => Set<EWaybillHeader>();
        public DbSet<EWaybillDetail> EWaybillDetail => Set<EWaybillDetail>();
        public DbSet<MiscTypeMaster> MiscTypeMaster => Set<MiscTypeMaster>();
        public DbSet<MiscMaster> MiscMaster => Set<MiscMaster>();
        public DbSet<AccountTypeMaster> AccountTypeMaster => Set<AccountTypeMaster>();
        public DbSet<GlAccountMaster> GlAccountMaster => Set<GlAccountMaster>();

        // US-GL02-07 type-ahead — per-user favourites + recently-used (relational store).
        public DbSet<GlAccountFavourite> GlAccountFavourite => Set<GlAccountFavourite>();
        public DbSet<GlAccountRecentUse> GlAccountRecentUse => Set<GlAccountRecentUse>();

        // US-GL02-FR-008a — COA freeze flag the DB triggers read (one row per company).
        public DbSet<CoaFreezeState> CoaFreezeState => Set<CoaFreezeState>();

        // US-GL02-08B — change-request + dual-approval unfreeze workflow that drives the 08a freeze state.
        public DbSet<CoaChangeRequest> CoaChangeRequest => Set<CoaChangeRequest>();
        public DbSet<CoaUnfreezeRequest> CoaUnfreezeRequest => Set<CoaUnfreezeRequest>();

        public DbSet<CurrencyForexConfig> CurrencyForexConfig => Set<CurrencyForexConfig>();

        // US-GL03-01 — Financial Year + auto-generated 13 periods (12 monthly + Period 13 adjustment).
        public DbSet<FinancialYearMaster> FinancialYearMaster => Set<FinancialYearMaster>();
        public DbSet<FinancialPeriodMaster> FinancialPeriodMaster => Set<FinancialPeriodMaster>();

        // US-GL03-02 — One-way period status machine + CFO/SysAdmin dual-approval reversal audit.
        public DbSet<PeriodStatusOverride> PeriodStatusOverride => Set<PeriodStatusOverride>();

        // Cost Centre master & 3-level hierarchy (US-GL05-01)
        public DbSet<CostCentre> CostCentre => Set<CostCentre>();

        // Profit Centre master & 2-level hierarchy (US-GL05-02)
        public DbSet<ProfitCentre> ProfitCentre => Set<ProfitCentre>();

        // COA bulk import/export (GL02-FR-006)
        public DbSet<GlAccountImportLog> GlAccountImportLog => Set<GlAccountImportLog>();
        public DbSet<GlAccountImportError> GlAccountImportError => Set<GlAccountImportError>();

        // Transactional outbox (SQL) — drained to the bus by the shared SqlOutboxProcessorJob.
        public DbSet<FinanceManagement.Domain.Entities.Outbox.OutboxMessage> OutboxMessages => Set<FinanceManagement.Domain.Entities.Outbox.OutboxMessage>();

        // Schedule III line-item & sub-total configuration (US-GL02-03A)
        public DbSet<ScheduleIIIHeader> ScheduleIIIHeader => Set<ScheduleIIIHeader>();
        public DbSet<ScheduleIIIDetail> ScheduleIIIDetail => Set<ScheduleIIIDetail>();
        public DbSet<ScheduleIIISection> ScheduleIIISection => Set<ScheduleIIISection>();
        public DbSet<ScheduleIIISectionItem> ScheduleIIISectionItem => Set<ScheduleIIISectionItem>();
        public DbSet<ScheduleIIISubTotal> ScheduleIIISubTotal => Set<ScheduleIIISubTotal>();
        public DbSet<ScheduleIIISubTotalFormula> ScheduleIIISubTotalFormula => Set<ScheduleIIISubTotalFormula>();

        // Tax code catalogue + GL linkage (US-GL02-05A / 05B)
        public DbSet<TaxCodeMaster> TaxCodeMaster => Set<TaxCodeMaster>();
        public DbSet<TaxCodeRateVersion> TaxCodeRateVersion => Set<TaxCodeRateVersion>();
        public DbSet<TaxAccountLinkage> TaxAccountLinkage => Set<TaxAccountLinkage>();

        // GSTR-1 / GSTR-3B section master + account-range mapping
        public DbSet<GstrSectionMaster> GstrSectionMaster => Set<GstrSectionMaster>();
        public DbSet<GstrSectionAccountLinkage> GstrSectionAccountLinkage => Set<GstrSectionAccountLinkage>();

        // Voucher-type configuration master + dedicated number series + allowed account types (US-GL01-02)
        public DbSet<VoucherTypeMaster> VoucherTypeMaster => Set<VoucherTypeMaster>();
        public DbSet<VoucherTypeAccountType> VoucherTypeAccountType => Set<VoucherTypeAccountType>();
        public DbSet<VoucherTypeNumberSeries> VoucherTypeNumberSeries => Set<VoucherTypeNumberSeries>();

        // Journal Voucher (GL01) — core transaction, masters & operational logs (US-GL01-*)
        public DbSet<AccountingPeriod> AccountingPeriod => Set<AccountingPeriod>();
        public DbSet<RecurringJournalTemplateHeader> RecurringJournalTemplateHeader => Set<RecurringJournalTemplateHeader>();
        public DbSet<RecurringJournalTemplateDetail> RecurringJournalTemplateDetail => Set<RecurringJournalTemplateDetail>();
        public DbSet<JournalThresholdRule> JournalThresholdRule => Set<JournalThresholdRule>();
        public DbSet<JournalHeader> JournalHeader => Set<JournalHeader>();
        public DbSet<JournalDetail> JournalDetail => Set<JournalDetail>();
        public DbSet<LedgerBalance> LedgerBalance => Set<LedgerBalance>();
        public DbSet<SequenceGapScanLog> SequenceGapScanLog => Set<SequenceGapScanLog>();
        public DbSet<RecurringGenerationLog> RecurringGenerationLog => Set<RecurringGenerationLog>();
        public DbSet<SecurityViolationLog> SecurityViolationLog => Set<SecurityViolationLog>();
        public DbSet<JournalFlag> JournalFlag => Set<JournalFlag>();
        public DbSet<JournalImportBatch> JournalImportBatch => Set<JournalImportBatch>();
        public DbSet<JournalImportError> JournalImportError => Set<JournalImportError>();

        // Property-level change trail (IActivityTracked entities)
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        // US-GL02-09 — immutable, field-level statutory audit trail for the COA structural masters
        // and governance entities (IAuditTrailed). Written only by AccountAuditTrailSaveChangesInterceptor.
        public DbSet<AccountAuditTrail> AccountAuditTrails => Set<AccountAuditTrail>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var currentTime = _timeZoneService.GetCurrentTime();
            var userId = _ipAddressService.GetUserId();
            var userName = _ipAddressService.GetUserName();
            var ipAddress = _ipAddressService.GetUserIPAddress();

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedDate = currentTime;
                    entry.Entity.CreatedByName = userName;
                    entry.Entity.CreatedIP = ipAddress;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedBy = userId;
                    entry.Entity.ModifiedDate = currentTime;
                    entry.Entity.ModifiedByName = userName;
                    entry.Entity.ModifiedIP = ipAddress;
                }
            }
        }
    }
}
