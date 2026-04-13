using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Locations;

namespace FixedAssetManagement.IntegrationTests.Repositories.Locations
{
    [Collection("DatabaseCollection")]
    public sealed class LocationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public LocationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private LocationCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static Location BuildEntity(
            string code = "LOC001",
            string name = "Test Location",
            int unitId = 1,
            int departmentId = 1) =>
            new Location
            {
                Code = code,
                LocationName = name,
                Description = "Test desc",
                UnitId = unitId,
                DepartmentId = departmentId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        // CREATE

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity("LOC_P", "PersistMe"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Locations.FirstAsync(x => x.Id == result.Id);
            saved.Code.Should().Be("LOC_P");
            saved.LocationName.Should().Be("PersistMe");
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var first = await CreateRepository(ctx).CreateAsync(BuildEntity("LOC_S1"));
            var second = await CreateRepository(ctx).CreateAsync(BuildEntity("LOC_S2"));

            second.SortOrder.Should().Be(first.SortOrder + 1);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Locations.FirstAsync(x => x.Id == result.Id);
            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
        }

        // UPDATE

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("LOC_U1", "Original"));
            ctx.ChangeTracker.Clear();

            var updateModel = new Location
            {
                Id = created.Id,
                Code = "LOC_U1",
                LocationName = "Updated",
                Description = "Updated desc",
                SortOrder = created.SortOrder,
                UnitId = 1,
                DepartmentId = 1,
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(updateModel);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("LOC_U2", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Location
            {
                Id = created.Id,
                Code = "LOC_U2",
                LocationName = "Renamed",
                Description = "New",
                UnitId = 1,
                DepartmentId = 1,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            (await ctx.Locations.FirstAsync(x => x.Id == created.Id)).LocationName.Should().Be("Renamed");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new Location { Id = 9999, LocationName = "X" });

            result.Should().BeFalse();
        }

        // DELETE

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("LOC_D1"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id,
                new Location { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Locations.IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new Location { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(0);
        }

        // CheckForDuplicates

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Detect_Name_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            await CreateRepository(ctx).CreateAsync(BuildEntity("LOC_DUP", "DupName"));

            var (nameDup, _) = await CreateRepository(ctx).CheckForDuplicatesAsync("DupName", 999, 0);

            nameDup.Should().BeTrue();
        }
    }
}
