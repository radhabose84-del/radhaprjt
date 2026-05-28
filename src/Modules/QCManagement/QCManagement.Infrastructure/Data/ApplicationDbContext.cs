using Contracts.Interfaces;
using QCManagement.Application.Common.Interfaces;
using QCManagement.Domain.Common;
using QCManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace QCManagement.Infrastructure.Data
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

        // ── DbSets ────────────────────────────────────────────────────────────
        public DbSet<MiscTypeMaster> MiscTypeMaster { get; set; }
        public DbSet<MiscMaster> MiscMaster { get; set; }
        public DbSet<QualityParameter> QualityParameter { get; set; }
        public DbSet<QualityTemplate> QualityTemplate { get; set; }
        public DbSet<QualityTemplateParameter> QualityTemplateParameter { get; set; }
        public DbSet<QualitySpecification> QualitySpecification { get; set; }
        public DbSet<QualitySpecificationParameter> QualitySpecificationParameter { get; set; }

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
