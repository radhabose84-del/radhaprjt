using Contracts.Interfaces;
using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationTemplate;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationConfig;
using BackgroundService.Infrastructure.Repositories.MiscMaster;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.NotificationTemplate
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationTemplateQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationTemplateQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private NotificationTemplateQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new NotificationTemplateQueryRepository(conn);
        }

        private Mock<IIPAddressService> CreateMockIp()
        {
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            return mockIp;
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code = "NTQTYPE")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Notification Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "NTQCHAN")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = "Channel",
                    SortOrder = 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedNotificationConfigAsync(int eventTypeId, string moduleName = "TestModule")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new NotificationConfigCommandRepository(ctx, CreateMockIp().Object);
            return await repo.CreateAsync(new Domain.Entities.Notification.NotificationConfig
            {
                ModuleName = moduleName,
                NotificationEventTypeId = eventTypeId,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedNotificationTemplateAsync(
            int notifTypeId,
            int configId,
            string subjectTemplate = "Test Subject",
            string languageCode = "en")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new NotificationTemplateCommandRepository(ctx, CreateMockIp().Object);
            return await repo.CreateAsync(new Domain.Entities.Notification.NotificationTemplate
            {
                NotificationTypeId = notifTypeId,
                NotificationConfigId = configId,
                SubjectTemplate = subjectTemplate,
                HeaderTemplate = "Header",
                BodyTemplate = "Body",
                FooterTemplate = "Footer",
                LanguageCode = languageCode,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(@"
                DELETE FROM AppNotification.NotificationEventRule;
                DELETE FROM AppNotification.NotificationLevelHierarchy;
                DELETE FROM AppNotification.NotificationTemplate;
                DELETE FROM AppNotification.NotificationEventLog;
                DELETE FROM AppNotification.NotificationGroupMembers;
                DELETE FROM AppNotification.NotificationGroup;
                DELETE FROM AppNotification.NotificationConfig;
                DELETE FROM AppData.ApprovalRuleCondition;
                DELETE FROM AppData.RuleTargetOverride;
                DELETE FROM AppData.ApprovalRule;
                DELETE FROM AppData.ApprovalRequestLine;
                DELETE FROM AppData.ApprovalDocument;
                DELETE FROM AppData.ApprovalRequest;
                DELETE FROM AppData.ApprovalStepDepartmentMapping;
                DELETE FROM AppData.ApprovalStepUnitMapping;
                DELETE FROM AppData.ApprovalStepDetail;
                DELETE FROM AppData.WorkflowType;
                DELETE FROM AppData.MiscMaster;
                DELETE FROM AppData.MiscTypeMaster;
            ");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllNotificationTemplateAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "EMAIL");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTALL");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            await SeedNotificationTemplateAsync(notifTypeId, configId);

            var (items, total) = await CreateQueryRepo().GetAllNotificationTemplateAsync(1, 10, null);

            items.Should().NotBeEmpty();
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllNotificationTemplateAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "DELALL");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTDA");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            var templateId = await SeedNotificationTemplateAsync(notifTypeId, configId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Notification.NotificationTemplate { IsDeleted = IsDelete.Deleted };
            await new NotificationTemplateCommandRepository(ctx, CreateMockIp().Object).DeleteAsync(templateId, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllNotificationTemplateAsync(1, 10, null);

            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "GETBYID");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTGBI");
            var configId = await SeedNotificationConfigAsync(eventTypeId, "SalesModule");
            var templateId = await SeedNotificationTemplateAsync(notifTypeId, configId, "My Subject", "en");

            var result = await CreateQueryRepo().GetByIdAsync(templateId);

            result.Should().NotBeNull();
            result.SubjectTemplate.Should().Be("My Subject");
            result.NotificationTypeId.Should().Be(notifTypeId);
            result.NotificationConfigId.Should().Be(configId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "DELBID");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTDB");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            var templateId = await SeedNotificationTemplateAsync(notifTypeId, configId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var deleteEntity = new Domain.Entities.Notification.NotificationTemplate { IsDeleted = IsDelete.Deleted };
            await new NotificationTemplateCommandRepository(ctx, CreateMockIp().Object).DeleteAsync(templateId, deleteEntity);

            var result = await CreateQueryRepo().GetByIdAsync(templateId);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "NFEXST");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTNE");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            var templateId = await SeedNotificationTemplateAsync(notifTypeId, configId);

            var result = await CreateQueryRepo().NotFoundAsync(templateId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeFalse();
        }
    }
}
