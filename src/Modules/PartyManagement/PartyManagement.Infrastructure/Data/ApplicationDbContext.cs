using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Contracts.Interfaces;
using PartyManagement.Application.Common.Interfaces;
using PartyManagement.Domain.Common;
using PartyManagement.Domain.Entities;
using PartyManagement.Domain.Entities.Outbox;
using PartyManagement.Infrastructure.Data.Configurations;
using PartyManagement.Infrastructure.Data.Configurations.Outbox;

namespace PartyManagement.Infrastructure.Data
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

        public DbSet<MiscTypeMaster> MiscTypeMaster { get; set; }
        public DbSet<MiscMaster> MiscMaster { get; set; }
        public DbSet<PartyGroup> PartyGroup { get; set; }
        public DbSet<PartyMaster> PartyMaster { get; set; }
        public DbSet<PartyType> PartyType { get; set; }
        public DbSet<PartyContact> PartyContact { get; set; }
        public DbSet<PartyAddress> PartyAddress { get; set; }
        public DbSet<PartyBank> PartyBank { get; set; }
        public DbSet<PartyDocument> PartyDocument { get; set; }
        public DbSet<PartyActivityLog> PartyActivityLog { get; set; }
        public DbSet<PartyUnitCompanyMapping> PartyUnitCompanyMapping { get; set; }
        public DbSet<BankAccount> BankAccount { get; set; }
        public DbSet<BankMaster> BankMaster { get; set; }
        public DbSet<SalesType> SalesType { get; set; }
        public DbSet<AgentConfig> AgentConfig { get; set; }
        public DbSet<TransportDetail> TransportDetail { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
            modelBuilder.ApplyConfiguration(new PartyGroupConfiguration());
            modelBuilder.ApplyConfiguration(new PartyMasterConfiguration());
            modelBuilder.ApplyConfiguration(new PartyTypeConfiguration());
            modelBuilder.ApplyConfiguration(new PartyContactConfiguration());
            modelBuilder.ApplyConfiguration(new PartyAddressConfiguration());
            modelBuilder.ApplyConfiguration(new PartyBankConfiguration());
            modelBuilder.ApplyConfiguration(new PartyDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new PartyActivityLogConfiguration());
            modelBuilder.ApplyConfiguration(new PartyUnitCompanyMappingConfiguration());
            modelBuilder.ApplyConfiguration(new BankAccountConfiguration());
            modelBuilder.ApplyConfiguration(new BankMasterConfiguration());
            modelBuilder.ApplyConfiguration(new SalesTypeConfiguration());
            modelBuilder.ApplyConfiguration(new AgentConfigConfiguration());
            modelBuilder.ApplyConfiguration(new TransportDetailConfiguration());
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
