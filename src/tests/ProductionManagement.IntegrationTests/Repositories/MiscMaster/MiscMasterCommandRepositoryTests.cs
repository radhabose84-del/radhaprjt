using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public MiscMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedTypeAsync(string code = "MM_TYP")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == code);
            if (existing != null) return existing.Id;
            var t = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code, Description = code,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(t);
            await ctx.SaveChangesAsync();
            return t.Id;
        }

        private async Task<Domain.Entities.MiscMaster> BuildEntityAsync(string code = "MM_C1", string desc = "Misc 1")
        {
            var typeId = await SeedTypeAsync();
            return new Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = code,
                Description = desc,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("MMC1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("MMC2", "DescX"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.MiscMaster.FirstAsync(x => x.Id == id);

            saved.Code.Should().Be("MMC2");
            saved.Description.Should().Be("DescX");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var typeId = await SeedTypeAsync();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("MMU1", "Old"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = id,
                MiscTypeId = typeId,
                Description = "New",
                SortOrder = 7,
                IsActive = Status.Inactive
            });
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.MiscMaster.FirstAsync(x => x.Id == id);
            reloaded.Description.Should().Be("New");
            reloaded.SortOrder.Should().Be(7);
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = 9999999, Description = "ghost", SortOrder = 1, IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("MMD1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_IsDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("MMD2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.MiscMaster.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Max_For_Type()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var typeId = await SeedTypeAsync();
            var e1 = await BuildEntityAsync("MMS1");
            e1.SortOrder = 5;
            var e2 = await BuildEntityAsync("MMS2");
            e2.SortOrder = 11;
            await CreateRepo(ctx).CreateAsync(e1);
            await CreateRepo(ctx).CreateAsync(e2);

            var result = await CreateRepo(ctx).GetMaxSortOrderAsync(typeId);

            result.Should().Be(11);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Zero_When_Empty()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var typeId = await SeedTypeAsync();

            var result = await CreateRepo(ctx).GetMaxSortOrderAsync(typeId);

            result.Should().Be(0);
        }
    }
}
