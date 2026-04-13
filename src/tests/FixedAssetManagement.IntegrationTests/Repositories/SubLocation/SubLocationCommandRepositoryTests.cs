using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Locations;
using FAM.Infrastructure.Repositories.SubLocation;

namespace FixedAssetManagement.IntegrationTests.Repositories.SubLocation
{
    [Collection("DatabaseCollection")]
    public sealed class SubLocationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SubLocationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SubLocationCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedLocationAsync(string code = "LOC_SUB1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new LocationCommandRepository(ctx);
            var result = await repo.CreateAsync(new Location
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

        private static FAM.Domain.Entities.SubLocation BuildEntity(
            int locationId,
            string code = "SLOC001",
            string name = "Sub Location") =>
            new FAM.Domain.Entities.SubLocation
            {
                Code = code,
                SubLocationName = name,
                Description = "Test desc",
                UnitId = 1,
                DepartmentId = 1,
                LocationId = locationId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var locationId = await SeedLocationAsync("SLOC_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(locationId));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var locationId = await SeedLocationAsync("SLOC_C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(locationId, "SLOC_P", "PersistMe"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SubLocations.FirstAsync(x => x.Id == result.Id);
            saved.Code.Should().Be("SLOC_P");
            saved.SubLocationName.Should().Be("PersistMe");
            saved.LocationId.Should().Be(locationId);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var locationId = await SeedLocationAsync("SLOC_C3");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(locationId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SubLocations.FirstAsync(x => x.Id == result.Id);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var locationId = await SeedLocationAsync("SLOC_U1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(locationId, "SLOC_U", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new FAM.Domain.Entities.SubLocation
            {
                Id = created.Id,
                Code = "SLOC_U",
                SubLocationName = "Renamed",
                Description = "New",
                UnitId = 1,
                DepartmentId = 1,
                LocationId = locationId,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            (await ctx.SubLocations.FirstAsync(x => x.Id == created.Id)).SubLocationName.Should().Be("Renamed");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new FAM.Domain.Entities.SubLocation { Id = 9999, SubLocationName = "X" });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var locationId = await SeedLocationAsync("SLOC_D1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(locationId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id,
                new FAM.Domain.Entities.SubLocation { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.SubLocations.IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new FAM.Domain.Entities.SubLocation { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var locationId = await SeedLocationAsync("SLOC_E1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(locationId, "SLOC_EX"));

            (await CreateRepository(ctx).ExistsByCodeAsync("SLOC_EX")).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_When_Excluded()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var locationId = await SeedLocationAsync("SLOC_E2");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(locationId, "SLOC_EX2"));

            (await CreateRepository(ctx).ExistsByCodeAsync("SLOC_EX2", created.Id)).Should().BeFalse();
        }
    }
}
