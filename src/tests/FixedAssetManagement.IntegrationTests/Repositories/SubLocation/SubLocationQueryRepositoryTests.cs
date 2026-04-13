using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Locations;
using FAM.Infrastructure.Repositories.SubLocation;

namespace FixedAssetManagement.IntegrationTests.Repositories.SubLocation
{
    [Collection("DatabaseCollection")]
    public sealed class SubLocationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SubLocationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SubLocationQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SubLocationQueryRepository(conn);
        }

        private async Task<int> SeedLocationAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new LocationCommandRepository(ctx).CreateAsync(new Location
            {
                Code = code,
                LocationName = "Loc " + code,
                UnitId = 1,
                DepartmentId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedEntityAsync(int locationId, string code = "SLOCQ_001", string name = "Sub Q")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new SubLocationCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.SubLocation
            {
                Code = code,
                SubLocationName = name,
                Description = "Q desc",
                UnitId = 1,
                DepartmentId = 1,
                LocationId = locationId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllSubLocationAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L1");
            await SeedEntityAsync(locationId);

            var (items, total) = await CreateQueryRepo().GetAllSubLocationAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllSubLocationAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L2");
            await SeedEntityAsync(locationId, "SLOCQ_A", "Alpha");
            await SeedEntityAsync(locationId, "SLOCQ_B", "Beta");

            var (items, _) = await CreateQueryRepo().GetAllSubLocationAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].SubLocationName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetAllSubLocationAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L3");
            var id = await SeedEntityAsync(locationId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SubLocationCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.SubLocation { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllSubLocationAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L4");
            var id = await SeedEntityAsync(locationId, "SLOCQ_ID", "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Code.Should().Be("SLOCQ_ID");
        }

        [Fact]
        public async Task GetBySubLocationNameAsync_Should_Return_Match()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L5");
            await SeedEntityAsync(locationId, "SLOCQ_N", "ByName");

            var result = await CreateQueryRepo().GetBySubLocationNameAsync("ByName", 1, locationId, 1);

            result.Should().NotBeNull();
            result!.Code.Should().Be("SLOCQ_N");
        }

        [Fact]
        public async Task GetBySubLocationNameAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L6");

            var result = await CreateQueryRepo().GetBySubLocationNameAsync("NoSuch", 1, locationId, 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBySubLocationCodeAsync_Should_Return_Match()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L7");
            await SeedEntityAsync(locationId, "SLOCQ_CODE", "ByCode");

            var result = await CreateQueryRepo().GetBySubLocationCodeAsync("SLOCQ_CODE", 1, locationId, 1);

            result.Should().NotBeNull();
            result!.SubLocationName.Should().Be("ByCode");
        }

        [Fact]
        public async Task GetSubLocation_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L8");
            await SeedEntityAsync(locationId, "SLOCQ_AC", "AutoSub");

            var result = await CreateQueryRepo().GetSubLocation("Auto");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task IsParentLocationActiveAsync_Should_Return_True_For_Active_Location()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L9");

            (await CreateQueryRepo().IsParentLocationActiveAsync(locationId)).Should().BeTrue();
        }

        [Fact]
        public async Task IsParentLocationActiveAsync_Should_Return_False_For_NotFound()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo().IsParentLocationActiveAsync(9999)).Should().BeFalse();
        }

        [Fact]
        public async Task IsSubLocationLinkedAsync_Should_Return_False_When_No_Children()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L10");
            var id = await SeedEntityAsync(locationId);

            (await CreateQueryRepo().IsSubLocationLinkedAsync(id)).Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTablesAsync();
            var locationId = await SeedLocationAsync("SLOCQ_L11");
            var id = await SeedEntityAsync(locationId);

            (await CreateQueryRepo().SoftDeleteValidationAsync(id)).Should().BeFalse();
        }
    }
}
