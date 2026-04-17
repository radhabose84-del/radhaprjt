using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Infrastructure.Repositories.ItemSpecificationValue;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemSpecificationValue
{
    [Collection("DatabaseCollection")]
    public sealed class ItemSpecificationValueCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemSpecificationValueCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemSpecificationValueCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> EnsureSpecMasterAsync(string name = "ISV_CMD")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.ItemSpecificationMaster.FirstOrDefaultAsync(m => m.SpecificationName == name);
            if (existing != null) return existing.Id;
            var maxOrder = await ctx.ItemSpecificationMaster.AnyAsync()
                ? await ctx.ItemSpecificationMaster.MaxAsync(m => m.Order)
                : 0;
            var m = new ItemSpecificationMaster
            {
                SpecificationCode = name.ToUpper()[..Math.Min(name.Length, 5)],
                SpecificationName = name,
                Order = maxOrder + 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ItemSpecificationMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationValue> BuildEntityAsync(
            string value)
        {
            var masterId = await EnsureSpecMasterAsync();
            return new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationValue
            {
                SpecificationMasterId = masterId,
                SpecificationValue = value,
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

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CmdVal1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("CmdVal2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ItemSpecificationValue.FirstAsync(x => x.Id == id);

            saved.SpecificationValue.Should().Be("CmdVal2");
            saved.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await BuildEntityAsync("Before");
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var masterId = entity.SpecificationMasterId;
            var result = await CreateRepo(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationValue
            {
                Id = id,
                SpecificationMasterId = masterId,
                SpecificationValue = "After",
                IsActive = Status.Inactive
            });
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.ItemSpecificationValue.FirstAsync(x => x.Id == id);
            reloaded.SpecificationValue.Should().Be("After");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(new InventoryManagement.Domain.Entities.Item.ItemDetail.Variant.ItemSpecificationValue
            {
                Id = 9999999,
                SpecificationMasterId = 1,
                SpecificationValue = "ghost",
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SD1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("SD2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.ItemSpecificationValue.FirstAsync(x => x.Id == id);
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
