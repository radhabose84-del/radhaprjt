using GateEntryManagement.Infrastructure.Repositories.MiscMaster;
using GateEntryManagement.Infrastructure.Repositories.MiscTypeMaster;
using GateEntryManagement.Infrastructure.Data;

namespace GateEntryManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscMasterCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedMiscTypeAsync(ApplicationDbContext ctx)
        {
            var repo = new MiscTypeMasterCommandRepository(ctx);
            return await repo.CreateAsync(new GateEntryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "TESTTYPE",
                Description = "Test Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private static GateEntryManagement.Domain.Entities.MiscMaster BuildEntity(
            int miscTypeId,
            string code = "MISC001",
            string description = "Test Misc",
            int sortOrder = 1) =>
            new GateEntryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MISC001", "Test Misc", 5));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("MISC001");
            saved.Description.Should().Be("Test Misc");
            saved.MiscTypeId.Should().Be(miscTypeId);
            saved.SortOrder.Should().Be(5);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var entityToUpdate = new GateEntryManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                Description = "Updated Description",
                SortOrder = 10,
                IsActive = Status.Inactive
            };
            var result = await CreateRepository(ctx).UpdateAsync(entityToUpdate);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var updated = await ctx.MiscMaster.FirstAsync(x => x.Id == id);
            updated.Description.Should().Be("Updated Description");
            updated.SortOrder.Should().Be(10);
            updated.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "ORIG001"));
            ctx.ChangeTracker.Clear();

            var entityToUpdate = new GateEntryManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                Code = "CHANGED",
                Description = "Different Name",
                SortOrder = 1,
                IsActive = Status.Active
            };
            await CreateRepository(ctx).UpdateAsync(entityToUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MiscMaster.FirstAsync(x => x.Id == id);
            updated.Code.Should().Be("ORIG001");
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Zero_When_Empty()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            var maxOrder = await CreateRepository(ctx).GetMaxSortOrderAsync(miscTypeId);

            maxOrder.Should().Be(0);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Max_Value()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var miscTypeId = await SeedMiscTypeAsync(ctx);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MISC001", "First", 3));
            ctx.ChangeTracker.Clear();
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MISC002", "Second", 7));
            ctx.ChangeTracker.Clear();
            await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MISC003", "Third", 5));
            ctx.ChangeTracker.Clear();

            var maxOrder = await CreateRepository(ctx).GetMaxSortOrderAsync(miscTypeId);

            maxOrder.Should().Be(7);
        }
    }
}
