using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Application.Common.Interfaces;
using ProductionManagement.Domain.Common;
using ProductionManagement.Domain.Entities;

namespace ProductionManagement.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<CountGroup> CountGroup => Set<CountGroup>();
        public DbSet<CountMaster> CountMaster => Set<CountMaster>();
        public DbSet<YarnType> YarnType => Set<YarnType>();
        public DbSet<MiscTypeMaster> MiscTypeMaster => Set<MiscTypeMaster>();
        public DbSet<MiscMaster> MiscMaster => Set<MiscMaster>();
        public DbSet<LotMaster> LotMaster => Set<LotMaster>();
        public DbSet<PackType> PackType => Set<PackType>();
        public DbSet<ProcessMaster> ProcessMaster => Set<ProcessMaster>();
        public DbSet<QualityMaster> QualityMaster => Set<QualityMaster>();
        public DbSet<CertificationMaster> CertificationMaster => Set<CertificationMaster>();
        public DbSet<ProductionPackHeader> ProductionPackHeader => Set<ProductionPackHeader>();
        public DbSet<ProductionPackDetail> ProductionPackDetail => Set<ProductionPackDetail>();

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
