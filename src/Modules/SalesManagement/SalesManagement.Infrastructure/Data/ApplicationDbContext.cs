using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Entities.Outbox;
using SalesManagement.Infrastructure.Data.Configurations;
using SalesManagement.Infrastructure.Data.Configurations.Outbox;



namespace SalesManagement.Infrastructure.Data
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

        // ── DbSets ────────────────────────────────────────────────────────────
        public DbSet<SalesOrganisation> SalesOrganisation { get; set; }
        public DbSet<SalesChannel> SalesChannel { get; set; }
        public DbSet<BusinessUnit> BusinessUnit { get; set; }
        public DbSet<SalesSegment> SalesSegment { get; set; }
        public DbSet<SalesOffice> SalesOffice { get; set; }
        public DbSet<SalesGroup> SalesGroup { get; set; }
  		public DbSet<ItemPriceMaster> ItemPriceMaster { get; set; }
        public DbSet<MiscTypeMaster> MiscTypeMaster { get; set; }
        public DbSet<MiscMaster> MiscMaster { get; set; }
        public DbSet<AgentCommissionConfig> AgentCommissionConfig { get; set; }
        public DbSet<AgentCustomerMapping> AgentCustomerMapping { get; set; }
        public DbSet<DispatchAddressMaster> DispatchAddressMaster { get; set; }
        public DbSet<MarketingOfficer> MarketingOfficer { get; set; }
        public DbSet<OfficerSalesGroup> OfficerSalesGroup { get; set; }
        public DbSet<DispatchAddressMapping> DispatchAddressMapping { get; set; }
        public DbSet<SalesContact> SalesContact { get; set; }
        public DbSet<SalesLead> SalesLead { get; set; }
        public DbSet<OfficerAgent> OfficerAgent { get; set; }
        public DbSet<SalesEnquiryHeader> SalesEnquiryHeader { get; set; }
        public DbSet<SalesEnquiryDetail> SalesEnquiryDetail { get; set; }
        public DbSet<SalesQuotationHeader> SalesQuotationHeader { get; set; }
        public DbSet<SalesQuotationDetail> SalesQuotationDetail { get; set; }
        public DbSet<CustomerVisit> CustomerVisit { get; set; }
        public DbSet<CustomerVisitProduct> CustomerVisitProduct { get; set; }
        public DbSet<SalesOrderHeader> SalesOrderHeader { get; set; }
        public DbSet<SalesOrderDetail> SalesOrderDetail { get; set; }
        public DbSet<StockLedger> StockLedger { get; set; }
        public DbSet<MovementTypeConfig> MovementTypeConfig { get; set; }
        public DbSet<DispatchAdviceHeader> DispatchAdviceHeader { get; set; }
        public DbSet<DispatchAdviceDetail> DispatchAdviceDetail { get; set; }
        public DbSet<StoTypeMaster> StoTypeMaster { get; set; }
        public DbSet<StoHeader> StoHeader { get; set; }
        public DbSet<StoDetail> StoDetail { get; set; }
        public DbSet<DeliveryChallanHeader> DeliveryChallanHeader { get; set; }
        public DbSet<DeliveryChallanDetail> DeliveryChallanDetail { get; set; }
        public DbSet<InvoiceHeader> InvoiceHeader { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetail { get; set; }
        public DbSet<StoReceiptHeader> StoReceiptHeader { get; set; }
        public DbSet<StoReceiptDetail> StoReceiptDetail { get; set; }
        public DbSet<ComplaintHeader> ComplaintHeader { get; set; }
        public DbSet<ComplaintDetail> ComplaintDetail { get; set; }
        public DbSet<ComplaintDetailNature> ComplaintDetailNature { get; set; }

        // ── Outbox (SQL-based for workflow transaction atomicity) ─────────
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── Entity Configurations ───────────────────────────────────────
            modelBuilder.ApplyConfiguration(new SalesOrganisationConfiguration());
            modelBuilder.ApplyConfiguration(new SalesChannelConfiguration());
            modelBuilder.ApplyConfiguration(new BusinessUnitConfiguration());
            modelBuilder.ApplyConfiguration(new SalesSegmentConfiguration());
            modelBuilder.ApplyConfiguration(new SalesOfficeConfiguration());
            modelBuilder.ApplyConfiguration(new SalesGroupConfiguration());
			modelBuilder.ApplyConfiguration(new ItemPriceMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
            modelBuilder.ApplyConfiguration(new AgentCommissionConfigConfiguration());
            modelBuilder.ApplyConfiguration(new AgentCustomerMappingConfiguration());
            modelBuilder.ApplyConfiguration(new DispatchAddressMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MarketingOfficerConfiguration());
            modelBuilder.ApplyConfiguration(new OfficerSalesGroupConfiguration());
            modelBuilder.ApplyConfiguration(new DispatchAddressMappingConfiguration());
            modelBuilder.ApplyConfiguration(new SalesContactConfiguration());
            modelBuilder.ApplyConfiguration(new SalesLeadConfiguration());
            modelBuilder.ApplyConfiguration(new OfficerAgentConfiguration());
            modelBuilder.ApplyConfiguration(new SalesEnquiryHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new SalesEnquiryDetailConfiguration());
            modelBuilder.ApplyConfiguration(new SalesQuotationHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new SalesQuotationDetailConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerVisitConfiguration());
            modelBuilder.ApplyConfiguration(new CustomerVisitProductConfiguration());
            modelBuilder.ApplyConfiguration(new SalesOrderHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new SalesOrderDetailConfiguration());
            modelBuilder.ApplyConfiguration(new StockLedgerConfiguration());
            modelBuilder.ApplyConfiguration(new MovementTypeConfigConfiguration());
            modelBuilder.ApplyConfiguration(new DispatchAdviceHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new DispatchAdviceDetailConfiguration());
            modelBuilder.ApplyConfiguration(new StoTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new StoHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new StoDetailConfiguration());
            modelBuilder.ApplyConfiguration(new DeliveryChallanHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new DeliveryChallanDetailConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceDetailConfiguration());
            modelBuilder.ApplyConfiguration(new StoReceiptHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new StoReceiptDetailConfiguration());
            modelBuilder.ApplyConfiguration(new ComplaintHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new ComplaintDetailConfiguration());
            modelBuilder.ApplyConfiguration(new ComplaintDetailNatureConfiguration());
            // ── Outbox (SQL-based for workflow) ─────────────────────────────
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

            // OfficerAgent does not extend BaseEntity — audit fields populated separately
            foreach (EntityEntry<OfficerAgent> entry in ChangeTracker.Entries<OfficerAgent>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedIP = currentIp;
                    entry.Entity.CreatedDate = currentTime;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.CreatedByName = username;
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedIP = currentIp;
                    entry.Entity.ModifiedDate = currentTime;
                    entry.Entity.ModifiedBy = userId;
                    entry.Entity.ModifiedByName = username;
                }
            }
        }
    }
}

