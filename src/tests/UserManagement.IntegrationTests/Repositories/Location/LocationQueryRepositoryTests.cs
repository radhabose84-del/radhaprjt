using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Domain.Enums.Common;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.Infrastructure.Data;
using UserManagement.Infrastructure.Repositories.Location;
using UserManagement.IntegrationTests.Common;
using Xunit;

namespace UserManagement.IntegrationTests.Repositories.Location
{
    [Collection("DatabaseCollection")]
    public sealed class LocationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LocationQueryRepositoryTests(DbFixture fixture)
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

        private LocationQueryRepository CreateQueryRepo()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new LocationQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedAsync(ApplicationDbContext ctx, string code = "LOC-0001", string name = "Query Location",
            Enums.Status status = Enums.Status.Active)
        {
            var cmdRepo = new LocationCommandRepository(ctx);
            var newId = await cmdRepo.CreateAsync(new UserManagement.Domain.Entities.Location
            {
                Code = code,
                LocationName = name,
                Description = "Seed",
                IsActive = status,
                IsDeleted = Enums.IsDelete.NotDeleted
            });
            ctx.ChangeTracker.Clear();
            return newId;
        }

        private async Task ClearTestDataAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllLocationAsync_Should_Return_Seeded_Records()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            await SeedAsync(ctx, "LOC-0001", "Location Alpha");

            var (items, total) = await CreateQueryRepo().GetAllLocationAsync(1, 100, null);

            items.Should().Contain(d => d.Code == "LOC-0001");
            total.Should().BeGreaterThanOrEqualTo(1);
        }

        [Fact]
        public async Task GetAllLocationAsync_Should_Filter_By_SearchTerm()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            await SeedAsync(ctx, "LOC-0002", "Searchable Location");

            var (items, _) = await CreateQueryRepo().GetAllLocationAsync(1, 10, "Searchable");

            items.Should().HaveCount(1);
            items[0].LocationName.Should().Be("Searchable Location");
        }

        [Fact]
        public async Task GetAllLocationAsync_Should_Exclude_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var id = await SeedAsync(ctx, "LOC-0003", "Deleted Location");

            await using var ctx2 = CreateDbContext();
            await new LocationCommandRepository(ctx2).DeleteAsync(id, new UserManagement.Domain.Entities.Location { IsDeleted = Enums.IsDelete.Deleted });

            var (items, _) = await CreateQueryRepo().GetAllLocationAsync(1, 100, "Deleted Location");

            items.Should().NotContain(d => d.Id == id);
        }

        [Fact]
        public async Task GetLocationByIdAsync_Should_Return_Correct_Entity()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var id = await SeedAsync(ctx, "LOC-0004", "ById Location");

            var result = await CreateQueryRepo().GetLocationByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("LOC-0004");
            result.LocationName.Should().Be("ById Location");
        }

        [Fact]
        public async Task GetLocationByIdAsync_Should_Return_Null_For_NonExistent()
        {
            var result = await CreateQueryRepo().GetLocationByIdAsync(99999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLocationByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var id = await SeedAsync(ctx, "LOC-0005", "Soft Deleted Location");

            await using var ctx2 = CreateDbContext();
            await new LocationCommandRepository(ctx2).DeleteAsync(id, new UserManagement.Domain.Entities.Location { IsDeleted = Enums.IsDelete.Deleted });

            var result = await CreateQueryRepo().GetLocationByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            await SeedAsync(ctx, "LOC-0006", "Dup Location");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("LOC-0006");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_NonExistent()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("LOC-9999");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            var id = await SeedAsync(ctx, "LOC-0007", "Exists Location");

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
        public async Task GetAllLocationAsync_Autocomplete_Should_Exclude_Inactive()
        {
            await using var ctx = CreateDbContext();
            await ClearTestDataAsync();
            await SeedAsync(ctx, "LOC-0008", "Inactive Location", Enums.Status.Inactive);

            var results = await CreateQueryRepo().GetAllLocationAsync("Inactive Location");

            results.Should().BeEmpty();
        }
    }
}
