using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Data.Configurations;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;
using PurchaseManagement.Infrastructure.Data.Configurations.Quotation.RfqEntry;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using Infrastructure.Persistence.Configurations;
using PurchaseManagement.Infrastructure.Data.Configurations.Quotation.QuotationFinal;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;
using PurchaseManagement.Domain.Entities.PriceMaster;
using Infrastructure.Persistence.Configurations.PriceMaster;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using PurchaseManagement.Infrastructure.Data.Configurations.GRN.GateEntry;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using Infrastructure.Data.Configurations;
using PurchaseManagement.Domain.Entities.PurchaseOrder.Local;
using PurchaseManagement.Domain.Entities.GRN.GRNEntry;
using PurchaseManagement.Infrastructure.Data.Configurations.GRN.GRNEntry;
using PurchaseManagement.Domain.Entities.GRN.StockLedger;
using PurchaseManagement.Infrastructure.Data.Configurations.GRN.GRNEntry.StockLedger;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ServicePO;
using PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder;
using PurchaseManagement.Domain.Entities.MRS;
using PurchaseManagement.Infrastructure.Data.Configurations.MRS;
using PurchaseManagement.Infrastructure.Data.Configurations.Issue;
using PurchaseManagement.Domain.Entities.Issue;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;
using PurchaseManagement.Domain.PurchaseOrder;
using PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.ImportPO;
using PurchaseManagement.Infrastructure.Data.Configurations.Purchase;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Infrastructure.Data.Configurations.IssueReturn;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using Infrastructure.Data.Configurations.PurchaseOrder.BillEntry;
using PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.BillEntry;
using PurchaseManagement.Infrastructure.Persistence.Configurations;
using PurchaseManagement.Domain.Entities.Outbox;
using PurchaseManagement.Infrastructure.Data.Configurations.Outbox;

namespace PurchaseManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> dbContextOptions, IIPAddressService ipAddressService, ITimeZoneService timeZoneService)
            : base(dbContextOptions)
        {
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;

        }

        // public DbSet<AssetGroup> AssetGroup { get; set; } 
        public DbSet<MiscTypeMaster> MiscTypeMaster { get; set; }
        public DbSet<MiscMaster> MiscMaster { get; set; }
        public DbSet<IndentHeader> IndentHeader { get; set; }
        public DbSet<IndentDetail> IndentDetail { get; set; }
        public DbSet<IndentLog> IndentLog { get; set; }
        public DbSet<PaymentTermMaster> PaymentTermMasters { get; set; }
        public DbSet<PaymentTermInstallment> PaymentTermInstallment { get; set; }

        //Rfq
        public DbSet<RfqMaster> Rfqs { get; set; }
        public DbSet<RfqItem> RfqItems { get; set; }
        public DbSet<RfqSupplier> RfqSuppliers { get; set; }    
        //End Rfq   
        //QuotationEntry        
        public DbSet<QuotationHeader> QuotationHeaders { get; set; }
        public DbSet<QuotationDetail> QuotationDetails { get; set; }
        public DbSet<TnCTemplateMaster> TnCTemplateMaster { get; set; }
        public DbSet<TnCTemplateApplicability> TnCTemplateApplicability { get; set; }        
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<QuotationComparisonHeader> QuotationComparisonHeader { get; set; }
        public DbSet<QuotationComparisonDetail> QuotationComparisonDetail { get; set; }
        public DbSet<PriceMasterHeader> PriceMasterHeader { get; set; }
        public DbSet<PriceMasterDetail> PriceMasterDetail { get; set; }
        public DbSet<GateEntryHeader> GateEntryHeader { get; set; }
        public DbSet<GateEntryDetail> GateEntryDetail { get; set; }
     //PO
        public DbSet<PurchaseOrderHeader> PurchaseOrderHeaders   { get; set; }
        public DbSet<PurchaseLocalHeader> PurchaseLocalHeaders  { get; set; }
        public DbSet<PurchaseLocalDetail> PurchaseLocalDetails { get; set; }
        public DbSet<PurchasePaymentTerm> PurchasePaymentTerms  { get; set; }
        public DbSet<GrnHeader> GrnHeader  { get; set; }
        public DbSet<GrnDetail> GrnDetail  { get; set; }
        public DbSet<GrnPutAwayRule> GrnPutAwayRule  { get; set; }
        public DbSet<ServiceMaster> Services  { get; set; }
        public DbSet<StockLedger> StockLedger  { get; set; }
        public DbSet<MrsHeader> MrsHeader  { get; set; }
        public DbSet<MrsDetail> MrsDetail  { get; set; }
        public DbSet<SubStoreStockLedger> SubStoreStockLedger  { get; set; }
        public DbSet<IssueHeader> IssueHeader  { get; set; }
        public DbSet<IssueDetail> IssueDetail  { get; set; }

        public DbSet<PurchaseOrderServiceHeader> PurchaseOrderServiceHeader  { get; set; }
        public DbSet<PurchaseOrderServiceLine> PurchaseOrderServiceLine  { get; set; }        
        public DbSet<PurchaseOrderServiceSchedule> PurchaseOrderServiceSchedule  { get; set; }
        public DbSet<ExchangeRate> ExchangeRates  { get; set; }
        public DbSet<PortMaster> PortMasters  { get; set; }
        
        public DbSet<ServiceEntrySheet> ServiceEntrySheets  { get; set; }
        public DbSet<ServiceEntryActivity> ServiceEntryActivities  { get; set; }
        public DbSet<ImportPOHeader> ImportPOHeader  { get; set; }
        public DbSet<ImportPODetail> ImportPODetail  { get; set; }
        public DbSet<PurchaseDocument> PurchaseDocuments  { get; set; }
        public DbSet<IssueReturnHeader> IssueReturnHeader  { get; set; }
        public DbSet<IssueReturnDetail> IssueReturnDetail  { get; set; }
        public DbSet<PurchaseBillEntryHeader> PurchaseBillEntryHeaders  { get; set; }
        public DbSet<PurchaseBillEntryDetail> PurchaseBillEntryDetails  { get; set; }
        public DbSet<ServiceEntrySheetDocument> ServiceEntrySheetDocuments { get; set; }

        // Outbox Pattern
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.ApplyConfiguration(new AssetGroupConfiguration());           

            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
            modelBuilder.ApplyConfiguration(new IndentHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new IndentDetailConfiguration());
            modelBuilder.ApplyConfiguration(new IndentLogConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentTermConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentTermInstallmentConfiguration());
            //Rfq
            modelBuilder.ApplyConfiguration(new RfqConfiguration());
            modelBuilder.ApplyConfiguration(new RfqItemConfiguration());
            modelBuilder.ApplyConfiguration(new RfqSupplierConfiguration());

            //End Rfq
            modelBuilder.ApplyConfiguration(new TnCTemplateMasterConfiguration());
            modelBuilder.ApplyConfiguration(new TnCTemplateApplicabilityConfiguration());
            //QuotationEntry
            modelBuilder.ApplyConfiguration(new QuotationHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new QuotationDetailConfiguration());


            modelBuilder.ApplyConfiguration(new ActivityLogConfiguration());

            modelBuilder.ApplyConfiguration(new QuotationConfirmedHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new QuotationConfirmedDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PriceMasterHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new PriceMasterDetailConfiguration());
            modelBuilder.ApplyConfiguration(new GateEntryHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new GateEntryDetailConfiguration());
            //PO
            modelBuilder.ApplyConfiguration(new PurchaseOrderHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseLocalHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseLocalDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PurchasePaymentTermConfiguration());
            modelBuilder.ApplyConfiguration(new GrnHeaderDetailConfiguration());
            modelBuilder.ApplyConfiguration(new GrnDetailDetailConfiguration());
            modelBuilder.ApplyConfiguration(new GrnPutAwayRuleDetailConfiguration());

            //Service PO
            modelBuilder.ApplyConfiguration(new ServiceMasterConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseOrderServiceHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseOrderServiceLineConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseOrderServiceScheduleConfiguration());

            modelBuilder.ApplyConfiguration(new StockLedgerDetailConfiguration());
            modelBuilder.ApplyConfiguration(new ExchangeRateConfiguration());
            modelBuilder.ApplyConfiguration(new PortMasterConfiguration());

            modelBuilder.ApplyConfiguration(new MrsHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new MrsDetailConfiguration());
            modelBuilder.ApplyConfiguration(new SubStoreStockLedgerConfiguration());
            modelBuilder.ApplyConfiguration(new IssueHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new IssueDetailConfiguration());

            modelBuilder.ApplyConfiguration(new ServiceEntrySheetConfiguration());
            modelBuilder.ApplyConfiguration(new ServiceEntryActivityConfiguration());
            modelBuilder.ApplyConfiguration(new ImportPOHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new ImportPODetailConfiguration());
            modelBuilder.ApplyConfiguration(new DutyMasterConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new IssueReturnHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new IssueReturnDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseBillEntryHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseBillEntryDetailConfiguration());
            modelBuilder.ApplyConfiguration(new ServiceEntrySheetDocumentConfiguration());

            // Outbox Pattern
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

            
            // Global convention: set explicit precision/scale for all decimal properties
            // This prevents EF Core runtime warnings about silent truncation
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                if (property.GetPrecision() == null)
                {
                    property.SetPrecision(18);
                    property.SetScale(6);
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateIpFields();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateIpFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateIpFields()
        {
            string currentIp = _ipAddressService.GetSystemIPAddress();
            int userId = _ipAddressService.GetUserId();
            string username = _ipAddressService.GetUserName();
            var systemTimeZoneId = _timeZoneService.GetSystemTimeZone();
            var currentTime = _timeZoneService.GetCurrentTime(systemTimeZoneId);

            foreach (EntityEntry entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedIP").CurrentValue = currentIp;
                    entry.Property("CreatedDate").CurrentValue = currentTime;
                    entry.Property("CreatedBy").CurrentValue = userId;
                    entry.Property("CreatedByName").CurrentValue = username;
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("ModifiedIP").CurrentValue = currentIp;
                    entry.Property("ModifiedDate").CurrentValue = currentTime;
                    entry.Property("ModifiedBy").CurrentValue = userId;
                    entry.Property("ModifiedByName").CurrentValue = username;
                }
            }
        }
    }
}
