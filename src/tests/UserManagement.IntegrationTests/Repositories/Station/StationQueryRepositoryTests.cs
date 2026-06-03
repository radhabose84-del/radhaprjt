using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Station;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Station
{
    [Collection("DatabaseCollection")]
    public sealed class StationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StationQueryRepositoryTests(DbFixture fixture)
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
            tzMock.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(_fixture.ConnectionString)
                .Options;

            return new ApplicationDbContext(options, ipMock.Object, tzMock.Object);
        }

        private StationQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new StationQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedAsync(ApplicationDbContext ctx, string code = "STA-0001", string name = "Query Station",
            Enums.Status status = Enums.Status.Active)
        {
            var cmdRepo = new StationCommandRepository(ctx);
            var newId = await cmdRepo.CreateAsync(new UserManagement.Domain.Entities.Station
            {
                Code = code,
                StationName = name,
                Description = "Seed",
                IsActive = status,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();
            return newId;
        }

        private async Task ClearTestDataAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllStationAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            await SeedAsync(ctx, "STA-0001", "Station Alpha");

            var (items, total) = await CreateQueryRepo().GetAllStationAsync(1, 100, null);

            items.Should().Contain(d => d.Code == "STA-0001");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllStationAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            await SeedAsync(ctx, "STA-0002", "Searchable Station");

            var (items, _) = await CreateQueryRepo().GetAllStationAsync(1, 10, "Searchable");

            items.Should().HaveCount(1);
            items[0].StationName.Should().Be("Searchable Station");
        }

        [Fact]
        public async Task GetAllStationAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var id = await SeedAsync(ctx, "STA-0003", "Deleted Station");

            await using var ctx2 = CreateDbContext();
            await new StationCommandRepository(ctx2).DeleteAsync(id, new UserManagement.Domain.Entities.Station { IsDeleted = Enums.IsDelete.Deleted });

            var (items, _) = await CreateQueryRepo().GetAllStationAsync(1, 100, "Deleted Station");

            items.Should().NotContain(d => d.Id == id);
        }

        [Fact]
        public async Task GetStationByIdAsync_Should_Return_Correct_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var id = await SeedAsync(ctx, "STA-0004", "ById Station");

            var result = await CreateQueryRepo().GetStationByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("STA-0004");
            result.StationName.Should().Be("ById Station");
        }

        [Fact]
        public async Task GetStationByIdAsync_Should_Return_Null_For_NonExistent()
        {
            var result = await CreateQueryRepo().GetStationByIdAsync(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetStationByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var id = await SeedAsync(ctx, "STA-0005", "Soft Deleted Station");

            await using var ctx2 = CreateDbContext();
            await new StationCommandRepository(ctx2).DeleteAsync(id, new UserManagement.Domain.Entities.Station { IsDeleted = Enums.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetStationByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            await SeedAsync(ctx, "STA-0006", "Dup Station");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("STA-0006");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("STA-9999");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var id = await SeedAsync(ctx, "STA-0007", "Exists Station");

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            var notFound = await CreateQueryRepo().NotFoundAsync(99999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task GetAllStationAsync_Autocomplete_Should_Exclude_Inactive()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            await SeedAsync(ctx, "STA-0008", "Inactive Station", Enums.Status.Inactive);

            var results = await CreateQueryRepo().GetAllStationAsync("Inactive Station");

            results.Should().BeEmpty();
        }
    }
}
