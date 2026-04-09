using Contracts.Interfaces;
using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationConfig;
using BackgroundService.Infrastructure.Repositories.MiscMaster;
using BackgroundService.Infrastructure.Repositories.MiscTypeMaster;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.NotificationConfig
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationConfigCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationConfigCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private NotificationConfigCommandRepository CreateRepository(NotificationDbContext ctx)
        {
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            return new NotificationConfigCommandRepository(ctx, mockIp.Object);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code = "NCTYPE")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Notification Event Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "NCEVENT")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.Notification.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = "Notification Event",
                    SortOrder = 0,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            return entity.Id;
        }

        private static Domain.Entities.Notification.NotificationConfig BuildEntity(
            int notificationEventTypeId,
            string moduleName = "TestModule") =>
            new()
            {
                ModuleName = moduleName,
                NotificationEventTypeId = notificationEventTypeId,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(NotificationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync(@"
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

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId));

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId, "SalesModule"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.NotificationConfig.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved.ModuleName.Should().Be("SalesModule");
            saved.NotificationEventTypeId.Should().Be(miscMasterId);
            saved.UnitId.Should().Be(1);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.NotificationConfig.FirstOrDefaultAsync(x => x.Id == newId);

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
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(miscMasterId, "UpdatedModule");
            var result = await CreateRepository(ctx).UpdateAsync(newId, updated);

            result.Should().Be(1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId));
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(miscMasterId, "ChangedModule");
            await CreateRepository(ctx).UpdateAsync(newId, updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.NotificationConfig.FirstOrDefaultAsync(x => x.Id == newId);
            saved.ModuleName.Should().Be("ChangedModule");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_NegativeOne_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);

            var entity = BuildEntity(miscMasterId);
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
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Notification.NotificationConfig
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
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscMasterId));
            ctx.ChangeTracker.Clear();

            var deleteEntity = new Domain.Entities.Notification.NotificationConfig
            {
                IsDeleted = IsDelete.Deleted
            };
            await CreateRepository(ctx).DeleteAsync(newId, deleteEntity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.NotificationConfig
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

            var deleteEntity = new Domain.Entities.Notification.NotificationConfig
            {
                IsDeleted = IsDelete.Deleted
            };
            var result = await CreateRepository(ctx).DeleteAsync(9999, deleteEntity);

            result.Should().Be(-1);
        }
    }
}
