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
using PurchaseManagement.Domain.Entities.FreightRfq;
using PurchaseManagement.Infrastructure.Data.Configurations.FreightRfq;
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
using PurchaseManagement.Domain.Entities.RawMaterialPO;
using PurchaseManagement.Infrastructure.Data.Configurations.RawMaterialPO;
using PurchaseManagement.Domain.Entities.IssueReturn;
using PurchaseManagement.Infrastructure.Data.Configurations.IssueReturn;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;
using Infrastructure.Data.Configurations.PurchaseOrder.BillEntry;
using PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.BillEntry;
using PurchaseManagement.Infrastructure.Persistence.Configurations;
using PurchaseManagement.Domain.Entities.BlanketMaster;
using PurchaseManagement.Domain.Entities.ContractPOMaster;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BlanketPO;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ContractPO;
using PurchaseManagement.Infrastructure.Data.Configurations.ContractPO;
using BlanketHeaderConfig = PurchaseManagement.Infrastructure.Data.Configurations.BlanketMaster.BlanketHeaderConfiguration;
using BlanketDetailConfig = PurchaseManagement.Infrastructure.Data.Configurations.BlanketMaster.BlanketDetailConfiguration;
using BlanketScheduleConfig = PurchaseManagement.Infrastructure.Data.Configurations.BlanketMaster.BlanketScheduleConfiguration;
using PurchaseBlanketHeaderConfig = PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.BlanketPO.PurchaseBlanketHeaderConfiguration;
using PurchaseBlanketDetailConfig = PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.BlanketPO.PurchaseBlanketDetailConfiguration;
using PurchaseContractHeaderConfig = PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.ContractPO.PurchaseContractHeaderConfiguration;
using PurchaseContractDetailConfig = PurchaseManagement.Infrastructure.Data.Configurations.PurchaseOrder.ContractPO.PurchaseContractDetailConfiguration;
using PurchaseManagement.Domain.Entities.Outbox;
using PurchaseManagement.Infrastructure.Data.Configurations.Outbox;
using PurchaseManagement.Domain.Entities.PurchaseReturn;
using PurchaseManagement.Infrastructure.Data.Configurations.PurchaseReturn;
using PurchaseManagement.Domain.Entities.VendorEvaluation;
using PurchaseManagement.Infrastructure.Data.Configurations.VendorEvaluation;

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
        public DbSet<OCREntry> OCREntry { get; set; }
        public DbSet<OCRQualityParameter> OCRQualityParameter { get; set; }

        //Raw Material PO (OCR conversion)
        public DbSet<RawMaterialPOHeader> RawMaterialPOHeader { get; set; }
        public DbSet<RawMaterialPODetail> RawMaterialPODetail { get; set; }

        //Rfq
        public DbSet<RfqMaster> Rfqs { get; set; }
        public DbSet<RfqItem> RfqItems { get; set; }
        public DbSet<RfqSupplier> RfqSuppliers { get; set; }
        public DbSet<RfqAttachment> RfqAttachments { get; set; }
        //End Rfq
        //QuotationEntry        
        public DbSet<QuotationHeader> QuotationHeaders { get; set; }
        public DbSet<QuotationDetail> QuotationDetails { get; set; }
        public DbSet<FreightRfqHeader> FreightRfqHeaders { get; set; }
        public DbSet<FreightRfqQuotation> FreightRfqQuotations { get; set; }
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

        // Contract PO (standalone contract master)
        public DbSet<ContractPOHeader> ContractPOHeaders { get; set; }
        public DbSet<ContractPODetail> ContractPODetails { get; set; }
        public DbSet<ContractPOReleaseHistory> ContractPOReleaseHistories { get; set; }

        // Contract PO (4th PO type — release PO under PurchaseOrderHeader)
        public DbSet<PurchaseContractHeader> PurchaseContractHeaders { get; set; }
        public DbSet<PurchaseContractDetail> PurchaseContractDetails { get; set; }

        // Blanket Master (standalone blanket agreement)
        public DbSet<BlanketHeader> BlanketHeaders { get; set; }
        public DbSet<BlanketDetail> BlanketDetails { get; set; }
        public DbSet<BlanketSchedule> BlanketSchedules { get; set; }

        // Blanket PO (5th PO type — release PO under PurchaseOrderHeader)
        public DbSet<PurchaseBlanketHeader> PurchaseBlanketHeaders { get; set; }
        public DbSet<PurchaseBlanketDetail> PurchaseBlanketDetails { get; set; }

        // Outbox Pattern
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

   // Purchase Return (RTV)
        public DbSet<ReturnType> ReturnTypes { get; set; }
        public DbSet<ReturnReason> ReturnReasons { get; set; }
        public DbSet<PurchaseReturnHeader> PurchaseReturnHeaders { get; set; }
        public DbSet<PurchaseReturnDetail> PurchaseReturnDetails { get; set; }        // Vendor Evaluation & Rating
        public DbSet<VendorEvaluationCriteria> VendorEvaluationCriteria { get; set; }
        public DbSet<VendorRatingGrade> VendorRatingGrades { get; set; }
        public DbSet<DeliveryScoreRule> DeliveryScoreRules { get; set; }
        public DbSet<VendorEvaluationHeader> VendorEvaluationHeaders { get; set; }
        public DbSet<VendorEvaluationDetail> VendorEvaluationDetails { get; set; }

        // Bale barcode series
        public DbSet<BarcodeSeries> BarcodeSeries { get; set; }

        // Bale barcode allocation
        public DbSet<BarcodeAllocation> BarcodeAllocation { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.ApplyConfiguration(new AssetGroupConfiguration());           

            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
            modelBuilder.ApplyConfiguration(new BarcodeSeriesConfiguration());
            modelBuilder.ApplyConfiguration(new BarcodeAllocationConfiguration());
            modelBuilder.ApplyConfiguration(new IndentHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new IndentDetailConfiguration());
            modelBuilder.ApplyConfiguration(new IndentLogConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentTermConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentTermInstallmentConfiguration());
            //Rfq
            modelBuilder.ApplyConfiguration(new RfqConfiguration());
            modelBuilder.ApplyConfiguration(new RfqItemConfiguration());
            modelBuilder.ApplyConfiguration(new RfqSupplierConfiguration());
            modelBuilder.ApplyConfiguration(new RfqAttachmentConfiguration());

            //End Rfq
            modelBuilder.ApplyConfiguration(new TnCTemplateMasterConfiguration());
            modelBuilder.ApplyConfiguration(new TnCTemplateApplicabilityConfiguration());
            //QuotationEntry
            modelBuilder.ApplyConfiguration(new QuotationHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new QuotationDetailConfiguration());

            modelBuilder.ApplyConfiguration(new FreightRfqHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new FreightRfqQuotationConfiguration());


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
            modelBuilder.ApplyConfiguration(new OCREntryConfiguration());
            modelBuilder.ApplyConfiguration(new OCRQualityParameterConfiguration());
            modelBuilder.ApplyConfiguration(new RawMaterialPOHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new RawMaterialPODetailConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new IssueReturnHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new IssueReturnDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseBillEntryHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseBillEntryDetailConfiguration());
            modelBuilder.ApplyConfiguration(new ServiceEntrySheetDocumentConfiguration());

            // Contract PO (standalone)
            modelBuilder.ApplyConfiguration(new ContractPOHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new ContractPODetailConfiguration());
            modelBuilder.ApplyConfiguration(new ContractPOReleaseHistoryConfiguration());

            // Contract PO (4th PO type under PurchaseOrderHeader)
            modelBuilder.ApplyConfiguration(new PurchaseContractHeaderConfig());
            modelBuilder.ApplyConfiguration(new PurchaseContractDetailConfig());

            // Blanket Master (standalone blanket agreement)
            modelBuilder.ApplyConfiguration(new BlanketHeaderConfig());
            modelBuilder.ApplyConfiguration(new BlanketDetailConfig());
            modelBuilder.ApplyConfiguration(new BlanketScheduleConfig());

            // Blanket PO (5th PO type under PurchaseOrderHeader)
            modelBuilder.ApplyConfiguration(new PurchaseBlanketHeaderConfig());
            modelBuilder.ApplyConfiguration(new PurchaseBlanketDetailConfig());

            // Outbox Pattern
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

              // Purchase Return (RTV)
            modelBuilder.ApplyConfiguration(new ReturnTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ReturnReasonConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseReturnHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new PurchaseReturnDetailConfiguration());
  // Vendor Evaluation & Rating
            modelBuilder.ApplyConfiguration(new VendorEvaluationCriteriaConfiguration());
            modelBuilder.ApplyConfiguration(new VendorRatingGradeConfiguration());
            modelBuilder.ApplyConfiguration(new DeliveryScoreRuleConfiguration());
            modelBuilder.ApplyConfiguration(new VendorEvaluationHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new VendorEvaluationDetailConfiguration());
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
