using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.CountMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.CountMaster
{
    [Collection("DatabaseCollection")]
    public sealed class CountMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public CountMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CountMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> EnsureMiscIdAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "CM_TYP");
            if (t == null)
            {
                t = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "CM_TYP", Description = "CM Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "CM_MM");
            if (m == null)
            {
                m = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "CM_MM", Description = "CM",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<Domain.Entities.CountMaster> BuildEntityAsync(string code = "CMC1")
        {
            var miscId = await EnsureMiscIdAsync();
            return new Domain.Entities.CountMaster
            {
                CountCode = code,
                CountValue = 30m,
                ShortName = "S",
                CountTypeId = miscId,
                CountCategoryId = miscId,
                CountDescription = "Cot",
                UOMId = 1,
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

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CMC1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CMC2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.CountMaster.FirstAsync(x => x.Id == id);

            saved.CountCode.Should().Be("CMC2");
            saved.CountValue.Should().Be(30m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CMU1"));
            ctx.ChangeTracker.Clear();

            var entity = await BuildEntityAsync("CMU1");
            entity.Id = id;
            entity.CountValue = 99m;
            entity.CountDescription = "Updated";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.CountMaster.FirstAsync(x => x.Id == id);
            reloaded.CountValue.Should().Be(99m);
            reloaded.CountDescription.Should().Be("Updated");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync("CMGH");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CMD1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CMD2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.CountMaster.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
