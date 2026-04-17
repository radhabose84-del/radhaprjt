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
    public sealed class NotificationConfigQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationConfigQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private NotificationConfigQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            return new NotificationConfigQueryRepository(conn, mockIp.Object);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code = "NCQTYPE")
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

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "NCQEVENT")
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

        private async Task<int> SeedNotificationConfigAsync(int notificationEventTypeId, string moduleName = "TestModule")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            var repo = new NotificationConfigCommandRepository(ctx, mockIp.Object);
            return await repo.CreateAsync(new Domain.Entities.Notification.NotificationConfig
            {
                ModuleName = moduleName,
                NotificationEventTypeId = notificationEventTypeId,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllNotificationConfigAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);
            await SeedNotificationConfigAsync(miscMasterId);

            var (items, total) = await CreateQueryRepo().GetAllNotificationConfigAsync(1, 10, null);

            items.Should().NotBeEmpty();
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllNotificationConfigAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);
            var configId = await SeedNotificationConfigAsync(miscMasterId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            var deleteEntity = new Domain.Entities.Notification.NotificationConfig { IsDeleted = IsDelete.Deleted };
            await new NotificationConfigCommandRepository(ctx, mockIp.Object).DeleteAsync(configId, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllNotificationConfigAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllNotificationConfigAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);
            await SeedNotificationConfigAsync(miscMasterId, "SalesModule");
            await SeedNotificationConfigAsync(miscMasterId, "PurchaseModule");

            var (items, total) = await CreateQueryRepo().GetAllNotificationConfigAsync(1, 10, "Sales");

            total.Should().Be(1);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId, "EVTCODE");
            var configId = await SeedNotificationConfigAsync(miscMasterId, "InventoryModule");

            var result = await CreateQueryRepo().GetByIdAsync(configId);

            result.Should().NotBeNull();
            result.ModuleName.Should().Be("InventoryModule");
            result.NotificationEventTypeId.Should().Be(miscMasterId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync();
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);
            var configId = await SeedNotificationConfigAsync(miscMasterId);

            await using var ctx = _fixture.CreateFreshDbContext();
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            var deleteEntity = new Domain.Entities.Notification.NotificationConfig { IsDeleted = IsDelete.Deleted };
            await new NotificationConfigCommandRepository(ctx, mockIp.Object).DeleteAsync(configId, deleteEntity);

            var result = await CreateQueryRepo().GetByIdAsync(configId);

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
            var miscMasterId = await SeedMiscMasterAsync(miscTypeId);
            var configId = await SeedNotificationConfigAsync(miscMasterId);

            var result = await CreateQueryRepo().NotFoundAsync(configId);

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
