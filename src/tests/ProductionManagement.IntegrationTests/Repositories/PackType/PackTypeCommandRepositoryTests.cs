using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.PackType;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.PackType
{
    [Collection("DatabaseCollection")]
    public sealed class PackTypeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public PackTypeCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PackTypeCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.PackType BuildEntity(string code = "PT_C1", string name = "Pack 1") =>
            new()
            {
                PackTypeCode = code,
                PackTypeName = name,
                NetWeight = 5m, TareWeight = 1m, GrossWeight = 6m,
                ConesPerBag = 10, ProductionAllowed = true,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PTC1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PTC2", "Big Pack"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.PackType.FirstAsync(x => x.Id == id);

            saved.PackTypeCode.Should().Be("PTC2");
            saved.PackTypeName.Should().Be("Big Pack");
            saved.NetWeight.Should().Be(5m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PTU1", "Old"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("PTU1", "New");
            entity.Id = id;
            entity.NetWeight = 99m;
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.PackType.FirstAsync(x => x.Id == id);
            reloaded.PackTypeName.Should().Be("New");
            reloaded.NetWeight.Should().Be(99m);
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity("PTGH");
            entity.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PTD1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PTD2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.PackType.FirstAsync(x => x.Id == id);
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
