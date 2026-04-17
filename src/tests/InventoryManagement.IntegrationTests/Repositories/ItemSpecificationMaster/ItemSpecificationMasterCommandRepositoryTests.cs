using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.ItemSpecificationMaster;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemSpecificationMasterTests
{
    [Collection("DatabaseCollection")]
    public sealed class ItemSpecificationMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemSpecificationMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemSpecificationMasterCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> NextOrderAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await ctx.ItemSpecificationMaster.AnyAsync()
                ? await ctx.ItemSpecificationMaster.MaxAsync(m => m.Order) + 1
                : 1;
        }

        private async Task<InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationMaster> BuildEntityAsync(
            string code = "CMD1",
            string name = "CmdName1")
        {
            var ord = await NextOrderAsync();
            return new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationMaster
            {
                SpecificationCode = code,
                SpecificationName = name,
                Order = ord,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SC1", "Name1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SC2", "Name2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ItemSpecificationMaster.FirstAsync(x => x.Id == id);

            saved.SpecificationCode.Should().Be("SC2");
            saved.SpecificationName.Should().Be("Name2");
            saved.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("SC3", "Name3");
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var ord = await NextOrderAsync();
            var updated = new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationMaster
            {
                Id = id,
                SpecificationName = "Renamed",
                Order = ord,
                IsActive = Status.Inactive
            };
            var result = await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.ItemSpecificationMaster.FirstAsync(x => x.Id == id);
            reloaded.SpecificationName.Should().Be("Renamed");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_Code()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("IMMUT", "Orig");
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationMaster
            {
                Id = id,
                SpecificationCode = "CHANGED",
                SpecificationName = "NewName",
                Order = await NextOrderAsync(),
                IsActive = Status.Active
            };
            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.ItemSpecificationMaster.FirstAsync(x => x.Id == id);
            reloaded.SpecificationCode.Should().Be("IMMUT");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationMaster
            {
                Id = 9999999,
                SpecificationName = "ghost",
                Order = await NextOrderAsync(),
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SD1", "SDName1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SD2", "SDName2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.ItemSpecificationMaster.FirstAsync(x => x.Id == id);
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
