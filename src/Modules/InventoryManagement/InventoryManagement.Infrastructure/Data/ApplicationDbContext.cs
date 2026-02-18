using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item;
using InventoryManagement.Infrastructure.Data.Configurations.Item;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Data.Configurations;
using InventoryManagement.Infrastructure.Data.Configurations.Budget;
using InventoryManagement.Domain.Entities.Budget;
using Microsoft.Identity.Client;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail.Variant;
using InventoryManagement.Infrastructure.Data.Configurations.Item.ItemDetail.Templates;
using InventoryManagement.Domain.Entities.item.ItemDetail.Templates;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;
using InventoryManagement.Domain.Entities.Item.PutAway;
using InventoryManagement.Domain.Entities.Stock;
using InventoryManagement.Infrastructure.Data.Configurations.Stock;
using InventoryManagement.Domain.Entities.MRS;
using InventoryManagement.Domain.Entities.Issue;
using InventoryManagement.Infrastructure.Data.Configurations.MRS;
using InventoryManagement.Infrastructure.Data.Issue;

namespace InventoryManagement.Infrastructure.Data
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

         public DbSet<ItemGroup> ItemGroup { get; set; } 
         public DbSet<ItemCategory> ItemCategory { get; set; } 

		 public DbSet<MiscTypeMaster> MiscTypeMaster { get; set; }
         public DbSet<MiscMaster> MiscMaster { get; set; }        
         public DbSet<HSNMaster> HSNMaster { get; set; }
         public DbSet<UOM> UOMs { get; set; }
         public DbSet<UOMConversion> UOMConversions { get; set; }
        public DbSet<BudgetMaster> BudgetMaster { get; set; }
        public DbSet<BudgetDetail> BudgetDetail { get; set; }
        public DbSet<BudgetLog> BudgetLog { get; set; }
        // Item related DbSets
        public DbSet<ItemMaster> ItemMaster { get; set; }        
        public DbSet<ItemSupplier> ItemSupplier { get; set; } 
        public DbSet<ItemManufacture> ItemManufacture { get; set; } 
        public DbSet<ItemPurchase> ItemPurchase { get; set; } 
        public DbSet<ItemInventory> ItemInventory { get; set; } 
        public DbSet<ItemQuality> ItemQuality { get; set; }         
        public DbSet<ItemVariantValue> ItemVariantValue { get; set; } 
        public DbSet<PutAwayStrategy> PutAwayStrategy { get; set; }
        public DbSet<ItemLog> ItemLog { get; set; } 
        public DbSet<ItemUOM> ItemUOMs { get; set; } 
        public DbSet<InspectionParameter> InspectionParameter { get; set; } 
        public DbSet<InspectionTemplate> InspectionTemplate { get; set; }         
        public DbSet<PutAwayRule> PutAwayRule { get; set; } 
        public DbSet<ItemVariantAttribute> ItemVariantAttribute { get; set; }
        public DbSet<StockLedger> StockLedger { get; set; } 
        public DbSet<SubStoreStockLedger> SubStoreStockLedger { get; set; } 
        public DbSet<MrsHeader> MrsHeader  { get; set; }
        public DbSet<MrsDetail> MrsDetail  { get; set; }
        public DbSet<IssueHeader> IssueHeader  { get; set; }
        public DbSet<IssueDetail> IssueDetail  { get; set; }

        
        //End  Item related DbSets


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new ItemGroupConfiguration());
            modelBuilder.ApplyConfiguration(new ItemCategoryConfiguration());

            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
            modelBuilder.ApplyConfiguration(new BudgetMasterConfiguration());
            modelBuilder.ApplyConfiguration(new BudgetDetailConfiguration());
            modelBuilder.ApplyConfiguration(new BudgetLogConfiguration());
            modelBuilder.ApplyConfiguration(new HSNMasterConfiguration());
            modelBuilder.ApplyConfiguration(new UOMConfiguration());
            modelBuilder.ApplyConfiguration(new UOMConversionConfiguration());
            //Item
            modelBuilder.ApplyConfiguration(new ItemMasterConfiguration());
            modelBuilder.ApplyConfiguration(new ItemSupplierConfiguration());
            modelBuilder.ApplyConfiguration(new ItemManufactureConfiguration());
            modelBuilder.ApplyConfiguration(new ItemPurchaseConfiguration());
            modelBuilder.ApplyConfiguration(new ItemInventoryConfiguration());
            modelBuilder.ApplyConfiguration(new ItemQualityConfiguration());
            modelBuilder.ApplyConfiguration(new ItemVariantValueConfiguration());
            modelBuilder.ApplyConfiguration(new ItemUOMConfiguration());
            modelBuilder.ApplyConfiguration(new ItemLogConfiguration());
            modelBuilder.ApplyConfiguration(new InspectionParameterConfiguration());
            modelBuilder.ApplyConfiguration(new InspectionTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new PutAwayRuleConfiguration());
            modelBuilder.ApplyConfiguration(new PutAwayStrategyConfiguration());
            modelBuilder.ApplyConfiguration(new ItemVariantAttributeConfiguration());
            modelBuilder.ApplyConfiguration(new StockLedgerConfiguration());
            modelBuilder.ApplyConfiguration(new SubStoreStockLedgerConfiguration());
            modelBuilder.ApplyConfiguration(new MrsHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new MrsDetailConfiguration());
            modelBuilder.ApplyConfiguration(new IssueHeaderConfiguration());
            modelBuilder.ApplyConfiguration(new IssueDetailConfiguration());



            
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

