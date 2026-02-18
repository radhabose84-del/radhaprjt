using Microsoft.EntityFrameworkCore;
using MaintenanceManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Infrastructure.Data.Configurations;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;
using MaintenanceManagement.Infrastructure.Data.Configurations.WorkOrderMaster;
using MaintenanceManagement.Domain.Entities.Power;
using MaintenanceManagement.Infrastructure.Data.Configurations.Power;
using MaintenanceManagement.Domain.Entities.Outbox;

namespace MaintenanceManagement.Infrastructure.Data
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
        
         public DbSet<CostCenter> CostCenter { get; set; } 
         public DbSet<WorkCenter> WorkCenter { get; set; } 
        public DbSet<MachineGroup> MachineGroup { get ; set; }
        public DbSet<MiscTypeMaster> MiscTypeMaster { get; set; }
        public DbSet<MiscMaster> MiscMaster { get; set; }
        public DbSet<ShiftMaster> ShiftMaster { get; set; }
        public DbSet<ShiftMasterDetail> ShiftMasterDetail { get; set; }
        public DbSet<MaintenanceType> MaintenanceType { get; set; }
        public DbSet<MaintenanceCategory> MaintenanceCategory { get; set; }
        public DbSet<ActivityMaster> ActivityMaster { get; set; }
        public DbSet<MachineMaster> MachineMaster { get; set; }
        public DbSet<ActivityMachineGroup>  ActivityMachineGroup { get; set; }
        public DbSet<MachineGroupUser>  MachineGroupUser { get; set; }
        public DbSet<WorkOrder>  WorkOrder { get; set; }
        public DbSet<WorkOrderItem>  WorkOrderItem { get; set; }
        public DbSet<WorkOrderTechnician>  WorkOrderTechnician { get; set; }
        public DbSet<WorkOrderSchedule>  WorkOrderSchedule { get; set; }
        public DbSet<WorkOrderActivity>  WorkOrderActivity { get; set; }
        public DbSet<WorkOrderCheckList>  WorkOrderCheckList { get; set;}
        

        public DbSet<ActivityCheckListMaster>  ActivityCheckListMaster { get; set; }
        public DbSet<PreventiveSchedulerHeader>  PreventiveSchedulerHdr { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequest { get; set;}
        public DbSet<PreventiveSchedulerDetail>  PreventiveSchedulerDtl { get; set; }
        public DbSet<PreventiveSchedulerItems>  PreventiveSchedulerItems { get; set; }
        public DbSet<PreventiveSchedulerActivity>  PreventiveSchedulerActivity { get; set; }
        public DbSet<ItemTransactions> SubStores { get; set; }
        public DbSet<StockLedger> StockLedger { get; set; }  

        public DbSet<FeederGroup> FeederGroup { get; set; } 
        public DbSet<Feeder> Feeder { get; set; }   
		public DbSet<PowerConsumption> PowerConsumption { get; set; }   
        public DbSet<MachineSpecification> MachineSpecification { get; set; }       
        public DbSet<GeneratorConsumption> GeneratorConsumption { get; set; }
        public DbSet<PreventiveScheduleLog> PreventiveScheduleLog  { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {
            modelBuilder.ApplyConfiguration(new MachineGroupConfiguration());
            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
            modelBuilder.ApplyConfiguration(new ShiftMasterConfiguration());
            modelBuilder.ApplyConfiguration(new ShiftMasterDetailsConfiguration());
            modelBuilder.ApplyConfiguration(new CostCenterConfiguration());
            modelBuilder.ApplyConfiguration(new WorkCenterConfiguration());
            modelBuilder.ApplyConfiguration(new MaintenanceTypeConfiguration());
            modelBuilder.ApplyConfiguration(new MaintenanceCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new ActivityMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MachineMasterConfiguration());
            modelBuilder.ApplyConfiguration(new ActivityMachineGroupConfiguration());

            modelBuilder.ApplyConfiguration(new WorkOrderConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderActivityConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderItemConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderScheduleConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderTechnicianConfiguration());
            modelBuilder.ApplyConfiguration(new WorkOrderCheckListConfiguration());

            modelBuilder.ApplyConfiguration(new ActivityCheckListMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MaintenanceRequestConfiguration());
            modelBuilder.ApplyConfiguration(new PreventiveSchedulerHdrConfiguration());
            modelBuilder.ApplyConfiguration(new PreventiveSchedulerDtlConfiguration());
            modelBuilder.ApplyConfiguration(new PreventiveSchedulerItemsConfiguration());
            modelBuilder.ApplyConfiguration(new PreventiveSchedulerActivityConfiguration());
            modelBuilder.ApplyConfiguration(new MachineGroupUserConfiguration());
            modelBuilder.ApplyConfiguration(new ItemTransactionsConfiguration());
            modelBuilder.ApplyConfiguration(new StockLedgerConfiguration());
            modelBuilder.ApplyConfiguration(new FeederGroupConfiguration());
            modelBuilder.ApplyConfiguration(new FeederConfiguration());
            modelBuilder.ApplyConfiguration(new PowerConsumptionConfiguration());
            modelBuilder.ApplyConfiguration(new MachineSpecificationConfiguration());
            modelBuilder.ApplyConfiguration(new GeneratorConsumptionConfiguration());
            modelBuilder.ApplyConfiguration(new PreventiveScheduleLogConfiguration());
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

