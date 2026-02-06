using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Infrastructure.Data.Configurations;

namespace WarehouseManagement.Infrastructure.Data
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
        
         public DbSet<WarehouseItemGroupMapping> GetWarehouseItemGroupMappings  { get; set; }
         public DbSet<WarehouseMaster> WarehouseMasters  { get; set; }
         public DbSet<RackMaster> RackMasters { get; set; }
         public DbSet<BinMaster> BinMasters { get; set; }
          
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new WarehouseMasterConfiguration());
            modelBuilder.ApplyConfiguration(new WarehouseItemGroupMappingConfiguration());
            modelBuilder.ApplyConfiguration(new RackMasterConfiguration());
            modelBuilder.ApplyConfiguration(new BinMasterConfiguration());

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
