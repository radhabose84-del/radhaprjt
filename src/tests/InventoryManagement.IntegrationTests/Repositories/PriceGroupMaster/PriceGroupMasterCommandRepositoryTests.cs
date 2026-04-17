using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.PriceGroupMaster;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.PriceGroupMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PriceGroupMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PriceGroupMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PriceGroupMasterCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static InventoryManagement.Domain.Entities.PriceGroupMaster BuildEntity(
            string code = "PGCR1",
            string name = "Cmd PG") =>
            new()
            {
                PriceGroupCode = code,
                PriceGroupName = name,
                Description = "desc",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity());

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PGCR2", "NameX"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.PriceGroupMaster.FirstAsync(x => x.Id == id);

            saved.PriceGroupCode.Should().Be("PGCR2");
            saved.PriceGroupName.Should().Be("NameX");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PGCR3"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.PriceGroupMaster.FirstAsync(x => x.Id == id);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("PGCR_U1", "Old"));
            ctx.ChangeTracker.Clear();

            var updated = new InventoryManagement.Domain.Entities.PriceGroupMaster
            {
                Id = id,
                PriceGroupName = "NewName",
                Description = "new desc",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active
            };
            var result = await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.PriceGroupMaster.FirstAsync(x => x.Id == id);
            reloaded.PriceGroupName.Should().Be("NewName");
            reloaded.Description.Should().Be("new desc");
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_PriceGroupCode()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("IMMUT1", "Orig"));
            ctx.ChangeTracker.Clear();

            var updated = new InventoryManagement.Domain.Entities.PriceGroupMaster
            {
                Id = id,
                PriceGroupCode = "CHANGED", // should be ignored
                PriceGroupName = "StillUpdated",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active
            };
            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.PriceGroupMaster.FirstAsync(x => x.Id == id);
            reloaded.PriceGroupCode.Should().Be("IMMUT1");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.PriceGroupMaster
            {
                Id = 9999999,
                PriceGroupName = "ghost",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_SoftDeleted()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DEL_U1"));
            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.PriceGroupMaster
            {
                Id = id,
                PriceGroupName = "after del",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        // --- SoftDeleteAsync ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("SD1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("SD2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.PriceGroupMaster.FirstAsync(x => x.Id == id);
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
        public async Task SoftDeleteAsync_Should_Return_False_When_AlreadyDeleted()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("SD3"));
            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
