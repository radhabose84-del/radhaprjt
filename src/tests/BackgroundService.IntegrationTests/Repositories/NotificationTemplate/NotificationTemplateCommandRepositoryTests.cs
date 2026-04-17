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
    public sealed class NotificationTemplateCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationTemplateCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private NotificationTemplateCommandRepository CreateRepository(NotificationDbContext ctx)
        {
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            return new NotificationTemplateCommandRepository(ctx, mockIp.Object);
        }

        private Mock<IIPAddressService> CreateMockIp()
        {
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            return mockIp;
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code = "NTTYPE")
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

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "NTCHAN")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = "Channel Type",
                    SortOrder = 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedNotificationConfigAsync(int notificationEventTypeId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new NotificationConfigCommandRepository(ctx, CreateMockIp().Object);
            return await repo.CreateAsync(new Domain.Entities.Notification.NotificationConfig
            {
                ModuleName = "TestModule",
                NotificationEventTypeId = notificationEventTypeId,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private static Domain.Entities.Notification.NotificationTemplate BuildEntity(
            int notificationTypeId,
            int notificationConfigId,
            string subjectTemplate = "Test Subject",
            string languageCode = "en") =>
            new()
            {
                NotificationTypeId = notificationTypeId,
                NotificationConfigId = notificationConfigId,
                SubjectTemplate = subjectTemplate,
                HeaderTemplate = "Header",
                BodyTemplate = "Body",
                FooterTemplate = "Footer",
                LanguageCode = languageCode,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(NotificationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "EMAIL");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTYPE");
            var configId = await SeedNotificationConfigAsync(eventTypeId);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(notifTypeId, configId));

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "SMS");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTSMS");
            var configId = await SeedNotificationConfigAsync(eventTypeId);

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity(notifTypeId, configId, "My Subject", "fr"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.NotificationTemplate.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved.SubjectTemplate.Should().Be("My Subject");
            saved.LanguageCode.Should().Be("fr");
            saved.NotificationTypeId.Should().Be(notifTypeId);
            saved.NotificationConfigId.Should().Be(configId);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "PUSH");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTPSH");
            var configId = await SeedNotificationConfigAsync(eventTypeId);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(notifTypeId, configId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.NotificationTemplate.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "UPD01");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTUPD");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(notifTypeId, configId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(notifTypeId, configId, "Updated Subject");
            var result = await CreateRepository(ctx).UpdateAsync(newId, updated);

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "CHNG01");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTCHG");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(notifTypeId, configId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(notifTypeId, configId, "Changed Subject", "de");
            await CreateRepository(ctx).UpdateAsync(newId, updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.NotificationTemplate.FirstOrDefaultAsync(x => x.Id == newId);
            saved.SubjectTemplate.Should().Be("Changed Subject");
            saved.LanguageCode.Should().Be("de");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_NegativeOne_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "NF01");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTNF");
            var configId = await SeedNotificationConfigAsync(eventTypeId);

            var entity = BuildEntity(notifTypeId, configId);
            var result = await CreateRepository(ctx).UpdateAsync(9999, entity);

            result.Should().Be(-1);
        }

        // --- DELETE (Soft Delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_One_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "DEL01");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTDEL");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(notifTypeId, configId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Notification.NotificationTemplate
            {
                IsDeleted = IsDelete.Deleted
            };
            var result = await CreateRepository(ctx).DeleteAsync(newId, deleteEntity);

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var notifTypeId = await SeedMiscMasterAsync(miscTypeId, "DELFLAG");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVTDF");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(notifTypeId, configId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Notification.NotificationTemplate
            {
                IsDeleted = IsDelete.Deleted
            };
            await CreateRepository(ctx).DeleteAsync(newId, deleteEntity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.NotificationTemplate
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted.Should().NotBeNull();
            deleted.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_NegativeOne_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var deleteEntity = new Domain.Entities.Notification.NotificationTemplate
            {
                IsDeleted = IsDelete.Deleted
            };
            var result = await CreateRepository(ctx).DeleteAsync(9999, deleteEntity);

            result.Should().Be(-1);
        }
    }
}
