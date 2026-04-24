using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.MiscMaster;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationConfig;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationLevelHierarchy;
using Contracts.Interfaces;

namespace BackgroundService.IntegrationTests.Repositories.NotificationLevelHierarchy
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationLevelHierarchyCommandTests
    {
        private readonly DbFixture _fixture;
        public NotificationLevelHierarchyCommandTests(DbFixture fixture) => _fixture = fixture;

        private NotificationLevelHierarchyCommand CreateRepo(NotificationDbContext ctx) => new(ctx);

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

        private async Task<(int ConfigId, int TargetTypeId, int ApprovalModeId)> SeedPrereqsAsync()
        {
            var miscTypeId = await SeedMiscTypeMasterAsync("NLH_TYP");
            var eventTypeId = await SeedMiscMasterAsync(miscTypeId, "EVT");
            var targetTypeId = await SeedMiscMasterAsync(miscTypeId, "TGT");
            var approvalModeId = await SeedMiscMasterAsync(miscTypeId, "AM");
            var configId = await SeedNotificationConfigAsync(eventTypeId);
            return (configId, targetTypeId, approvalModeId);
        }

        private async Task<int> SeedHierarchyAsync(
            int configId, int targetTypeId, int approvalModeId,
            int targetId = 10,
            string description = "desc",
            bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Domain.Entities.Notification.NotificationLevelHierarchy
            {
                NotificationConfigId = configId,
                TargetTypeId = targetTypeId,
                TargetId = targetId,
                ApprovalModeId = approvalModeId,
                Description = description,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.NotificationLevelHierarchy.Add(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        // --- INSERT ---

        [Fact]
        public async Task InsertAsync_Should_Return_True_And_Persist()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Domain.Entities.Notification.NotificationLevelHierarchy
            {
                NotificationConfigId = configId,
                TargetTypeId = targetTypeId,
                TargetId = 99,
                ApprovalModeId = approvalModeId,
                Description = "inserted",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var result = await CreateRepo(ctx).InsertAsync(entity);

            result.Should().BeTrue();
            entity.Id.Should().BeGreaterThan(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity_When_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            var id = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 42);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.TargetId.Should().Be(42);
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
        public async Task UpdateAsync_Should_Modify_Description()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            var id = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId);

            await using var ctx1 = _fixture.CreateFreshDbContext();
            var entity = await ctx1.NotificationLevelHierarchy.FirstAsync(x => x.Id == id);
            entity.Description = "updated";

            var ok = await CreateRepo(ctx1).UpdateAsync(entity);

            ok.Should().BeTrue();
            await using var ctx2 = _fixture.CreateFreshDbContext();
            var reloaded = await ctx2.NotificationLevelHierarchy.FirstAsync(x => x.Id == id);
            reloaded.Description.Should().Be("updated");
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Remove_Entity()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            var id = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId);

            await using var ctx1 = _fixture.CreateFreshDbContext();
            var entity = await ctx1.NotificationLevelHierarchy.FirstAsync(x => x.Id == id);

            var ok = await CreateRepo(ctx1).DeleteAsync(entity);

            ok.Should().BeTrue();
            await using var ctx2 = _fixture.CreateFreshDbContext();
            (await ctx2.NotificationLevelHierarchy.FirstOrDefaultAsync(x => x.Id == id))
                .Should().BeNull();
        }

        // --- ExistsByCodeAsync ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Matching_Triple_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 7);

            await using var ctx = _fixture.CreateFreshDbContext();
            var exists = await CreateRepo(ctx).ExistsByCodeAsync(configId, targetTypeId, 7);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_When_No_Match()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 7);

            await using var ctx = _fixture.CreateFreshDbContext();
            var exists = await CreateRepo(ctx).ExistsByCodeAsync(configId, targetTypeId, 999);

            exists.Should().BeFalse();
        }

        // --- ExistsByCodeExcludingIdAsync ---

        [Fact]
        public async Task ExistsByCodeExcludingIdAsync_Should_Exclude_Self()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            var id = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 5);

            await using var ctx = _fixture.CreateFreshDbContext();
            var exists = await CreateRepo(ctx).ExistsByCodeExcludingIdAsync(configId, targetTypeId, 5, currentId: id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByCodeExcludingIdAsync_Should_Return_True_For_Other()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            var id1 = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 5);
            var id2 = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 5);

            await using var ctx = _fixture.CreateFreshDbContext();
            var exists = await CreateRepo(ctx).ExistsByCodeExcludingIdAsync(configId, targetTypeId, 5, currentId: id1);

            exists.Should().BeTrue();
        }

        // --- NotFoundAsync ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Absent()
        {
            await _fixture.ClearAllTablesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).NotFoundAsync(9999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            var id = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).NotFoundAsync(id);

            result.Should().BeFalse();
        }

        // --- SoftDeleteValidation ---

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_True_When_NotDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            var id = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, isDeleted: false);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).SoftDeleteValidation(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_Deleted()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            var id = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, isDeleted: true);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).SoftDeleteValidation(id);

            result.Should().BeFalse();
        }

        // --- GetAllWithEventRuleAsync ---

        [Fact]
        public async Task GetAllWithEventRuleAsync_Should_Return_Paginated_NonDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 1);
            await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 2);
            await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 3, isDeleted: true);

            await using var ctx = _fixture.CreateFreshDbContext();
            var (items, total) = await CreateRepo(ctx).GetAllWithEventRuleAsync(1, 10, null);

            total.Should().Be(2);
            items.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllWithEventRuleAsync_Should_Filter_By_SearchTerm()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 1, description: "alpha hierarchy");
            await SeedHierarchyAsync(configId, targetTypeId, approvalModeId, targetId: 2, description: "beta hierarchy");

            await using var ctx = _fixture.CreateFreshDbContext();
            var (items, total) = await CreateRepo(ctx).GetAllWithEventRuleAsync(1, 10, "alpha");

            total.Should().Be(1);
            items.Should().ContainSingle().Which.Description.Should().Be("alpha hierarchy");
        }

        [Fact]
        public async Task GetByIdWithEventRuleAsync_Should_Return_With_Navigations()
        {
            await _fixture.ClearAllTablesAsync();
            var (configId, targetTypeId, approvalModeId) = await SeedPrereqsAsync();
            var id = await SeedHierarchyAsync(configId, targetTypeId, approvalModeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).GetByIdWithEventRuleAsync(id);

            result.Should().NotBeNull();
            result!.NotificationConfig.Should().NotBeNull();
            result.TargetType.Should().NotBeNull();
            result.ApprovalMode.Should().NotBeNull();
        }
    }
}
