using Contracts.Interfaces;
using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Locations;

namespace FixedAssetManagement.IntegrationTests.Repositories.Locations
{
    [Collection("DatabaseCollection")]
    public sealed class LocationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LocationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private LocationQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            return new LocationQueryRepository(conn, ipMock.Object);
        }

        private async Task<int> SeedEntityAsync(string code = "LOCQ_001", string name = "Loc Q", int unitId = 1, int departmentId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new LocationCommandRepository(ctx);
            var result = await repo.CreateAsync(new Location
            {
                Code = code,
                LocationName = name,
                Description = "Q desc",
                UnitId = unitId,
                DepartmentId = departmentId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllLocationListAsync_Should_Return_Seeded()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllLocationListAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllLocationListAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("LOCQ_A", "Alpha");
            await SeedEntityAsync("LOCQ_B", "Beta");

            var (items, _) = await CreateQueryRepo().GetAllLocationListAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].LocationName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetAllLocationListAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new LocationCommandRepository(ctx).DeleteAsync(id,
                new Location { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllLocationListAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("LOCQ_ID", "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.Code.Should().Be("LOCQ_ID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByLocationNameAsync_Should_Return_Match()
        {
            await ClearTableAsync();
            await SeedEntityAsync("LOCQ_N", "ByName", 1, 1);

            var result = await CreateQueryRepo().GetByLocationNameAsync("ByName", 1, 1);

            result.Should().NotBeNull();
            result!.Code.Should().Be("LOCQ_N");
        }

        [Fact]
        public async Task GetByLocationNameAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByLocationNameAsync("NoSuch", 1, 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetLocation_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("LOCQ_AC", "AutoLoc");

            var result = await CreateQueryRepo().GetLocation("Auto");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task IsLinkedWithSubLocationsAsync_Should_Return_False_When_No_Children()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            (await CreateQueryRepo().IsLinkedWithSubLocationsAsync(id)).Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            (await CreateQueryRepo().SoftDeleteValidationAsync(id)).Should().BeFalse();
        }
    }
}
