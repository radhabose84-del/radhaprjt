using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Contracts.Interfaces;
using ProjectManagement.Application.Common.Interfaces;
using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Entities;
using ProjectManagement.Domain.Entities.Outbox;
using ProjectManagement.Infrastructure.Data.Configurations;
using Microsoft.CodeAnalysis;

namespace ProjectManagement.Infrastructure.Data
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
        public DbSet<ProjectMaster> ProjectMaster { get; set; }

        public DbSet<ProjectDocument> ProjectDocument { get; set; }

        public DbSet<ProjectWorkBreakdownStructure> ProjectWorkBreakdownStructures { get; set; } = null!;
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {
            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectMasterConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectWorkBreakdownStructureConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.Outbox.OutboxMessageConfiguration());

            
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

