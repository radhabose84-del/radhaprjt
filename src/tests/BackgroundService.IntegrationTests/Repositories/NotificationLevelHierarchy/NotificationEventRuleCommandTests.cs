using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.MiscMaster;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationConfig;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationLevelHierarchy;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationTemplate;
using Contracts.Interfaces;

namespace BackgroundService.IntegrationTests.Repositories.NotificationLevelHierarchy
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationEventRuleCommandTests
    {
        private readonly DbFixture _fixture;
        public NotificationEventRuleCommandTests(DbFixture fixture) => _fixture = fixture;

        private NotificationEventRuleCommand CreateRepo(NotificationDbContext ctx) => new(ctx);

        private Mock<IIPAddressService> CreateMockIp()
        {
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            return mockIp;
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = code + " Desc",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = code + " Desc",
                    SortOrder = 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedNotificationConfigAsync(int eventTypeId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new NotificationConfigCommandRepository(ctx, CreateMockIp().Object);
            return await repo.CreateAsync(new Domain.Entities.Notification.NotificationConfig
            {
                ModuleName = "TestModule",
                NotificationEventTypeId = eventTypeId,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedTemplateAsync(int typeId, int configId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new NotificationTemplateCommandRepository(ctx, CreateMockIp().Object);
            return await repo.CreateAsync(new Domain.Entities.Notification.NotificationTemplate
            {
                NotificationTypeId = typeId,
                NotificationConfigId = configId,
                SubjectTemplate = "s",
                HeaderTemplate = "h",
                BodyTemplate = "b",
                FooterTemplate = "f",
                LanguageCode = "en",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedHierarchyAsync(int configId, int targetTypeId, int approvalModeId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Domain.Entities.Notification.NotificationLevelHierarchy
            {
                NotificationConfigId = configId,
                TargetTypeId = targetTypeId,
                TargetId = 1,
                ApprovalModeId = approvalModeId,
                Description = "h",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.NotificationLevelHierarchy.Add(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task<(int HierarchyId, int RecipientTypeId, int ChannelId, int TemplateId)> SeedPrereqsAsync()
        {
            var miscTypeId = await SeedMiscTypeMasterAsync("NER_TYP");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVT");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TGT");
            var approvalModeId = await SeedMiscMasterAsync(miscTypeId, "AM");
            var recipientTypeId = await SeedMiscMasterAsync(miscTypeId, "RCP");
            var channelId = await SeedMiscMasterAsync(miscTypeId, "CHN");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            var templateId = await SeedTemplateAsync(eventTypeId, configId);
            var hierarchyId = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId);
            return (hierarchyId, recipientTypeId, channelId, templateId);
        }

        private async Task<int> SeedRuleAsync(
            int hierarchyId, int recipientTypeId, int channelId, int templateId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Domain.Entities.Notification.NotificationEventRule
            {
                NotificationLevelHierarchyId = hierarchyId,
                RecipientTypeId = recipientTypeId,
                NotificationChannelId = channelId,
                TemplateId = templateId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.NotificationEventRule.Add(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        // --- INSERT ---

        [Fact]
        public async Task InsertAsync_Should_Return_True()
        {
            await _fixture.ClearAllTablesAsync();
            var (hierarchyId, recipientTypeId, channelId, templateId) = await SeedPrereqsAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Domain.Entities.Notification.NotificationEventRule
            {
                NotificationLevelHierarchyId = hierarchyId,
                RecipientTypeId = recipientTypeId,
                NotificationChannelId = channelId,
                TemplateId = templateId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var result = await CreateRepo(ctx).InsertAsync(entity);

            result.Should().BeTrue();
            entity.Id.Should().BeGreaterThan(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity()
        {
            await _fixture.ClearAllTablesAsync();
            var (hierarchyId, recipientTypeId, channelId, templateId) = await SeedPrereqsAsync();
            var id = await SeedRuleAsync(hierarchyId, recipientTypeId, channelId, templateId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.NotificationLevelHierarchyId.Should().Be(hierarchyId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await _fixture.ClearAllTablesAsync();
            var (hierarchyId, recipientTypeId, channelId, templateId) = await SeedPrereqsAsync();
            var id = await SeedRuleAsync(hierarchyId, recipientTypeId, channelId, templateId);

            await using var ctx1 = _fixture.CreateFreshDbContext();
            var entity = await ctx1.NotificationEventRule.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            var ok = await CreateRepo(ctx1).UpdateAsync(entity);

            ok.Should().BeTrue();
            await using var ctx2 = _fixture.CreateFreshDbContext();
            var reloaded = await ctx2.NotificationEventRule.FirstAsync(x => x.Id == id);
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        // --- DeleteByHierarchyIdAsync ---

        [Fact]
        public async Task DeleteByHierarchyIdAsync_Should_Remove_All_For_Hierarchy()
        {
            await _fixture.ClearAllTablesAsync();
            var (hierarchyId, recipientTypeId, channelId, templateId) = await SeedPrereqsAsync();
            await SeedRuleAsync(hierarchyId, recipientTypeId, channelId, templateId);
            await SeedRuleAsync(hierarchyId, recipientTypeId, channelId, templateId);

            await using var ctx1 = _fixture.CreateFreshDbContext();
            var ok = await CreateRepo(ctx1).DeleteByHierarchyIdAsync(hierarchyId);

            ok.Should().BeTrue();
            await using var ctx2 = _fixture.CreateFreshDbContext();
            (await ctx2.NotificationEventRule.CountAsync(x => x.NotificationLevelHierarchyId == hierarchyId))
                .Should().Be(0);
        }

        [Fact]
        public async Task DeleteByHierarchyIdAsync_Should_Return_True_When_NothingToDelete()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var ok = await CreateRepo(ctx).DeleteByHierarchyIdAsync(9999);

            ok.Should().BeTrue();
        }

        // --- DeleteRangeAsync ---

        [Fact]
        public async Task DeleteRangeAsync_Should_Remove_Supplied_Entities()
        {
            await _fixture.ClearAllTablesAsync();
            var (hierarchyId, recipientTypeId, channelId, templateId) = await SeedPrereqsAsync();
            var id1 = await SeedRuleAsync(hierarchyId, recipientTypeId, channelId, templateId);
            var id2 = await SeedRuleAsync(hierarchyId, recipientTypeId, channelId, templateId);

            await using var ctx1 = _fixture.CreateFreshDbContext();
            var entities = await ctx1.NotificationEventRule
                .Where(x => x.Id == id1 || x.Id == id2)
                .ToListAsync();

            var ok = await CreateRepo(ctx1).DeleteRangeAsync(entities);

            ok.Should().BeTrue();
            await using var ctx2 = _fixture.CreateFreshDbContext();
            (await ctx2.NotificationEventRule.CountAsync()).Should().Be(0);
        }
    }
}
