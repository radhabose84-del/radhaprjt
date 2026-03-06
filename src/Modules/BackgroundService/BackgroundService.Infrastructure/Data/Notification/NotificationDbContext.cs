using BackgroundService.Application.Notification.Common.Interfaces;
using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using BackgroundService.Domain.Entities.Workflow;
using BackgroundService.Infrastructure.Data.Notification.Configurations;
using BackgroundService.Infrastructure.Data.Workflow.Configurations;
using BackgroundService.Infrastructure.Persistence;
using BackgroundService.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using BackgroundService.Core.Domain.Entities.Notifications;

namespace BackgroundService.Infrastructure.Data.Notification
{
    public class NotificationDbContext : DbContext
    {
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService; 
        public NotificationDbContext(DbContextOptions<NotificationDbContext> options, IIPAddressService ipAddressService, ITimeZoneService timeZoneService)
        : base(options)
        {
            _ipAddressService = ipAddressService; 
            _timeZoneService = timeZoneService;     
        }

        public DbSet<NotificationConfig> NotificationConfig { get; set; }
        public DbSet<NotificationGroup> NotificationGroup { get; set; }
        public DbSet<NotificationGroupMembers> NotificationGroupMembers { get; set; }
        public DbSet<NotificationLevelHierarchy> NotificationLevelHierarchy { get; set; }
        public DbSet<NotificationEventRule> NotificationEventRule { get; set; }
        public DbSet<NotificationEventLog> NotificationEventLog { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplate { get; set; }
        public DbSet<MiscTypeMaster> MiscTypeMaster { get; set; }
        public DbSet<MiscMaster> MiscMaster { get; set; }
        public DbSet<WorkflowType> WorkflowType { get; set; }
        public DbSet<ApprovalStepDetail> ApprovalStepDetail { get; set; }
        public DbSet<ApprovalStepUnitMapping> ApprovalStepUnitMapping { get; set; }
        public DbSet<ApprovalRule> ApprovalRule { get; set; }        
        public DbSet<ApprovalRequest> ApprovalRequest { get; set; }
        public DbSet<ApprovalStepDepartmentMapping> ApprovalStepDepartmentMapping { get; set; }
        public DbSet<ApprovalDocument> ApprovalDocument { get; set; }
        public DbSet<ApprovalRequestLine> ApprovalRequestLine { get; set; }
        public DbSet<ApprovalRuleCondition> ApprovalRuleCondition { get; set; }
        public DbSet<RuleTargetOverride> RuleTargetOverride { get; set; }
        public DbSet<ApprovalDataField> ApprovalDataField { get; set; }
        public DbSet<NotificationTablePreset> NotificationTablePreset { get; set; }
        public DbSet<NotificationWhatsAppGroup> NotificationWhatsAppGroup { get; set; }
        public DbSet<InboxMessage> InboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new NotificationConfigConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationGroupConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationGroupMembersConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationLevelHierarchyConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationEventRuleConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationEventLogConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationTemplateConfiguration());
            modelBuilder.ApplyConfiguration(new MiscTypeMasterConfiguration());
            modelBuilder.ApplyConfiguration(new MiscMasterConfiguration());
            modelBuilder.ApplyConfiguration(new WorkflowTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalStepDetailConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalStepUnitMappingConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalRuleConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalRequestConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalStepDepartmentMappingConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalRequestLineConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalRuleConditionConfiguration());
            modelBuilder.ApplyConfiguration(new RuleTargetOverrideConfiguration());
            modelBuilder.ApplyConfiguration(new ApprovalDataFieldConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationTablePresetConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationWhatsAppGroupConfiguration());
            modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
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