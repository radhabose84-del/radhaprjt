using Contracts.Interfaces;
using BackgroundService.Infrastructure.Data.Notification;
using BackgroundService.Infrastructure.Repositories.Notification.NotificationGroup;
using Microsoft.Data.SqlClient;
using Dapper;

namespace BackgroundService.IntegrationTests.Repositories.NotificationGroup
{
    [Collection("DatabaseCollection")]
    public sealed class NotificationGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public NotificationGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private NotificationGroupQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            return new NotificationGroupQueryRepository(conn, mockIp.Object);
        }

        private async Task<int> SeedNotificationGroupAsync(string groupName = "Test Group")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            var repo = new NotificationGroupCommandRepository(ctx, mockIp.Object);
            return await repo.CreateAsync(new Domain.Entities.Notification.NotificationGroup
            {
                GroupName = groupName,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllNotificationGroupAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            await SeedNotificationGroupAsync();

            var (items, total) = await CreateQueryRepo().GetAllNotificationGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllNotificationGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var groupId = await SeedNotificationGroupAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            var deleteEntity = new Domain.Entities.Notification.NotificationGroup { IsDeleted = IsDelete.Deleted };
            await new NotificationGroupCommandRepository(ctx, mockIp.Object).DeleteAsync(groupId, deleteEntity);

            var (items, total) = await CreateQueryRepo().GetAllNotificationGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllNotificationGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            await SeedNotificationGroupAsync("Alpha Group");
            await SeedNotificationGroupAsync("Beta Group");

            var (items, total) = await CreateQueryRepo().GetAllNotificationGroupAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].GroupName.Should().Be("Alpha Group");
        }

        // --- GET BY ID (via GetAllNotificationGroupAsync) ---

        [Fact]
        public async Task GetAllNotificationGroupAsync_Should_Return_Correct_Dto_Fields()
        {
            await ClearTablesAsync();
            await SeedNotificationGroupAsync("Detailed Group");

            var (items, _) = await CreateQueryRepo().GetAllNotificationGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].GroupName.Should().Be("Detailed Group");
            items[0].Id.Should().BeGreaterThan(0);
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetNotificationGroupsAutoComplete_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            await SeedNotificationGroupAsync("Auto Group");
            await SeedNotificationGroupAsync("Other Group");

            var result = await CreateQueryRepo().GetNotificationGroupsAutoComplete("Auto");

            result.Should().HaveCount(1);
            result[0].GroupName.Should().Be("Auto Group");
        }

        [Fact]
        public async Task GetNotificationGroupsAutoComplete_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var groupId = await SeedNotificationGroupAsync("Inactive Group");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.NotificationGroup.FirstAsync(x => x.Id == groupId);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var result = await CreateQueryRepo().GetNotificationGroupsAutoComplete("Inactive");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetNotificationGroupsAutoComplete_Should_Return_All_When_Empty_Pattern()
        {
            await ClearTablesAsync();
            await SeedNotificationGroupAsync("Group One");
            await SeedNotificationGroupAsync("Group Two");

            var result = await CreateQueryRepo().GetNotificationGroupsAutoComplete("");

            result.Should().HaveCount(2);
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var groupId = await SeedNotificationGroupAsync();

            var result = await CreateQueryRepo().NotFoundAsync(groupId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().NotFoundAsync(9999);

            result.Should().BeFalse();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            await SeedNotificationGroupAsync("Duplicate Group");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("Duplicate Group");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NonExistent");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearTablesAsync();
            var groupId = await SeedNotificationGroupAsync("Self Group");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("Self Group", groupId);

            exists.Should().BeFalse();
        }
    }
}
