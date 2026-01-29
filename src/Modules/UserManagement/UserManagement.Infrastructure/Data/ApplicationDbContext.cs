using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Core.Application.Common.Interfaces;
using Core.Domain.Common;
using UserManagement.Infrastructure.Data.Configurations;


namespace UserManagement.Infrastructure.Data
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
        
        public DbSet<Entity> Entity { get; set; } 
        public DbSet<Unit> Unit { get; set; } 
        public DbSet<UnitAddress> UnitAddress { get; set; }
        public DbSet<UnitContacts> UnitContacts { get; set; }
        public DbSet<Department> Department { get; set; } 
        public DbSet<User> User { get; set; }
        public DbSet<UserRole> UserRole { get; set; } 
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyAddress> companyAddresses { get; set; }
        public DbSet<CompanyContact> CompanyContacts { get; set; }
        public DbSet<Division> Divisions { get; set; }
        public DbSet<Modules> Modules { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<RoleEntitlement> RoleEntitlements { get; set; }
        public DbSet<Countries> Countries { get; set; }
        public DbSet<States> States { get; set; }
        public DbSet<Cities> Cities { get; set; }
        public DbSet<PasswordLog> PasswordLogs { get; set; }
        public DbSet<UserRoleAllocation> UserRoleAllocations { get; set; }
         public DbSet<UserSessions> UserSession { get; set; }
 
        public DbSet<PasswordComplexityRule> PasswordComplexityRule { get; set; }
        public DbSet<AdminSecuritySettings> AdminSecuritySettings { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<TimeZones> TimeZones { get; set; }
        public DbSet<UserCompany> UserCompanies { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; } 
        public DbSet<Language> Languages { get; set; }
        public DbSet<FinancialYear> FinancialYear { get; set;}
        public DbSet<UserUnit> UserUnit { get; set; }
        public DbSet<RoleMenuPrivileges> RoleMenuPrivileges { get; set; } 
        public DbSet<RoleModule> RoleModules { get; set; }
        public DbSet<UserGroup> UserGroup { get; set; }
        public DbSet<RoleParent> RoleParent { get; set; }
        public DbSet<RoleChild> RoleChild { get; set; }
        public DbSet<UserDivision> UserDivision {get; set;}
        public DbSet<UserDepartment> UserDepartment {get; set;}
        public DbSet<MiscMaster> MiscMaster { get; set; } 
        public DbSet<MiscTypeMaster> MiscTypeMaster { get; set; }
        public DbSet<CustomField> CustomField { get; set; }
        public DbSet<CustomFieldOptionalValue> CustomFieldOptionalValue { get; set; }
        public DbSet<CustomFieldUnit> CustomFieldUnit { get; set; }
        public DbSet<CustomFieldMenu> CustomFieldMenu { get; set; }
        
        public DbSet<DepartmentGroup> DepartmentGroup { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UnitConfiguration());
            modelBuilder.ApplyConfiguration(new UnitAddressConfiguration());
            modelBuilder.ApplyConfiguration(new UnitContactsConfiguration());
            modelBuilder.ApplyConfiguration(new RoleEntitlementConfigurations());
            modelBuilder.ApplyConfiguration(new MenuConfiguration());
            modelBuilder.ApplyConfiguration(new ModulesConfiguration());
            modelBuilder.ApplyConfiguration(new CountryConfiguration());
            modelBuilder.ApplyConfiguration(new UnitConfiguration());
            modelBuilder.ApplyConfiguration(new CompanyConfiguration());
            modelBuilder.ApplyConfiguration(new CompanyAddressConfiguration());
            modelBuilder.ApplyConfiguration(new StateConfiguration());
            modelBuilder.ApplyConfiguration(new CityConfiguration());
            modelBuilder.ApplyConfiguration(new CompanyContactConfiguration());
            modelBuilder.ApplyConfiguration(new EntityConfigurations());
            modelBuilder.ApplyConfiguration(new DivisionConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleAllocationConfigurations());
            modelBuilder.ApplyConfiguration(new UserSessionConfiguration());

            modelBuilder.ApplyConfiguration(new PwdComplexityRuleConfiguration());
            modelBuilder.ApplyConfiguration(new AdminSecuritySettingsConfiguration());


            modelBuilder.ApplyConfiguration(new PasswordLogConfiguration());
            modelBuilder.ApplyConfiguration(new FinancialYearConfiguration());
            modelBuilder.ApplyConfiguration(new CurrencyConfiguration());
            modelBuilder.ApplyConfiguration(new TimeZonesConfiguration());
            modelBuilder.ApplyConfiguration(new UserCompanyConfiguration());
            modelBuilder.ApplyConfiguration(new CompanySettingsConfiguration());
            modelBuilder.ApplyConfiguration(new LanguageConfiguration());
            modelBuilder.ApplyConfiguration(new UserUnitConfiguration());
            modelBuilder.ApplyConfiguration(new RoleMenuPrivilegesConfiguration());
            modelBuilder.ApplyConfiguration(new RoleModuleConfiguration());
            modelBuilder.ApplyConfiguration(new UserGroupConfiguration());
            modelBuilder.ApplyConfiguration(new RoleParentConfiguration());
            modelBuilder.ApplyConfiguration(new RoleChildConfiguration());
            modelBuilder.ApplyConfiguration(new UserDivisionConfiguration());
            modelBuilder.ApplyConfiguration(new UserDepartmentConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new CustomFieldConfiguration());
            modelBuilder.ApplyConfiguration(new CustomFieldOptionalValueConfiguration());
            modelBuilder.ApplyConfiguration(new CustomFieldUnitConfiguration());
            modelBuilder.ApplyConfiguration(new CustomFieldMenuConfiguration());
            modelBuilder.ApplyConfiguration(new DepartmentGroupConfiguration());


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
                    entry.Property("CreatedAt").CurrentValue = currentTime;
                    entry.Property("CreatedBy").CurrentValue = userId;
                    entry.Property("CreatedByName").CurrentValue = username;
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Property("ModifiedIP").CurrentValue = currentIp;
                    entry.Property("ModifiedAt").CurrentValue = currentTime;
                    entry.Property("ModifiedBy").CurrentValue = userId;
                    entry.Property("ModifiedByName").CurrentValue = username;
                }
            }
        }
    }
}
