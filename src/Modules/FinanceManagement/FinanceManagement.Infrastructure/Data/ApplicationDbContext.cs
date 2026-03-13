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
        public DbSet<EInvoiceHeader> EInvoiceHeader => Set<EInvoiceHeader>();
        public DbSet<EInvoiceDetail> EInvoiceDetail => Set<EInvoiceDetail>();

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
