using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.IconMaster;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.IconMaster
{
    [Collection("DatabaseCollection")]
    public sealed class IconMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public IconMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ApplicationDbContext CreateDbContext()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetEntityId()).Returns(1);
            ipMock.Setup(x => x.GetGroupCode()).Returns("SUPER_ADMIN");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");

            var tzMock = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tzMock.Setup(x => x.GetSystemTimeZone()).Returns("UTC");

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private IconMasterQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(
            string keyword = "settings",
            string iconName = "SlSettings",
            string iconLibrary = "sl",
            int size = 18,
            string? style = "{\"animation\":\"spin\"}",
            Enums.Status isActive = Enums.Status.Active)
        {
            await using var ctx = CreateDbContext();
            var repo = new IconMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.IconMaster
            {
                Keyword = keyword,
                IconName = iconName,
                IconLibrary = iconLibrary,
                Size = size,
                Style = style,
                IsActive = isActive,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllIconMasterAsync_Should_Return_Seeded_Record_With_AllFields()
        {
            await ClearTableAsync();
            await SeedAsync("settings", "SlSettings", "sl", 18, "{\"animation\":\"spin\"}");

            var (items, total) = await CreateQueryRepo().GetAllIconMasterAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
            items[0].Keyword.Should().Be("settings");
            items[0].IconName.Should().Be("SlSettings");
            items[0].IconLibrary.Should().Be("sl");
            items[0].Size.Should().Be(18);
            items[0].Style.Should().Be("{\"animation\":\"spin\"}");
        }

        [Fact]
        public async Task GetAllIconMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = CreateDbContext();
            var deleteModel = new Domain.Entities.IconMaster { IsDeleted = Enums.IsDelete.Deleted };
            await new IconMasterCommandRepository(ctx).DeleteIconMasterAsync(id, deleteModel);

            var (items, total) = await CreateQueryRepo().GetAllIconMasterAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllIconMasterAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedAsync("settings", "SlSettings", "sl");
            await SeedAsync("dashboard", "MdOutlineDashboard", "md");

            var (items, _) = await CreateQueryRepo().GetAllIconMasterAsync(1, 10, "dash");

            items.Should().HaveCount(1);
            items[0].Keyword.Should().Be("dashboard");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedAsync("production", "MdOutlinePrecisionManufacturing", "md", 20);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Keyword.Should().Be("production");
            result.IconName.Should().Be("MdOutlinePrecisionManufacturing");
            result.Size.Should().Be(20);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedAsync();

            await using var ctx = CreateDbContext();
            var deleteModel = new Domain.Entities.IconMaster { IsDeleted = Enums.IsDelete.Deleted };
            await new IconMasterCommandRepository(ctx).DeleteIconMasterAsync(id, deleteModel);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- AUTOCOMPLETE (by keyword) ---

        [Fact]
        public async Task GetByKeywordAsync_Should_Return_Matching_With_AllFields()
        {
            await ClearTableAsync();
            await SeedAsync("settings", "SlSettings", "sl", 18, "{\"a\":1}");
            await SeedAsync("dashboard", "MdOutlineDashboard", "md");

            var results = await CreateQueryRepo().GetByKeywordAsync("settings");

            results.Should().HaveCount(1);
            results[0].IconName.Should().Be("SlSettings");
            results[0].IconLibrary.Should().Be("sl");
            results[0].Size.Should().Be(18);
            results[0].Style.Should().Be("{\"a\":1}");
        }

        [Fact]
        public async Task GetByKeywordAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            await SeedAsync("settings", "SlSettings", "sl", 18, "{\"a\":1}", Enums.Status.Inactive);

            var results = await CreateQueryRepo().GetByKeywordAsync("settings");

            results.Should().BeEmpty();
        }
    }
}
