using Contracts.Interfaces;
using GateEntryManagement.Application.Common.Interfaces;
using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;
using GateEntryManagement.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace GateEntryManagement.Infrastructure.Data
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

        public DbSet<MiscTypeMaster> MiscTypeMaster { get; set; } = null!;
        public DbSet<MiscMaster> MiscMaster { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
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
