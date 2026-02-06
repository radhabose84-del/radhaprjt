using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Domain.Common;


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
        
       
       

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {
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

