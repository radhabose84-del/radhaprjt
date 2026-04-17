using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item;
using InventoryManagement.Infrastructure.Repositories.Item.ItemGroup;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.ItemGroupTests
{
    [Collection("DatabaseCollection")]
    public sealed class ItemGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemGroupCommandRepository CreateRepo(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private static InventoryManagement.Domain.Entities.Item.ItemGroup BuildEntity(
            string code = "GRP1", string name = "GrpName") =>
            new()
            {
                ItemGroupCode = code,
                ItemGroupName = name,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("GC1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_UnitId_From_IpService()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity("GC2");
            entity.UnitId = 0;
            var id = await CreateRepo(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemGroup.FirstAsync(x => x.Id == id);
            saved.UnitId.Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("GC3", "Name3"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.ItemGroup.FirstAsync(x => x.Id == id);

            saved.ItemGroupCode.Should().Be("GC3");
            saved.ItemGroupName.Should().Be("Name3");
            saved.IsActive.Should().Be(Status.Active);
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("UPD1", "Orig"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).UpdateAsync(id, new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                ItemGroupCode = "UPD1",
                ItemGroupName = "Updated",
                IsActive = Status.Inactive
            });
            ctx.ChangeTracker.Clear();

            result.Should().Be(1);
            var reloaded = await ctx.ItemGroup.FirstAsync(x => x.Id == id);
            reloaded.ItemGroupName.Should().Be("Updated");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(9999999, BuildEntity("GH"));

            result.Should().Be(-1);
        }

        // --- DeleteAsync ---

        [Fact]
        public async Task DeleteAsync_Should_Return_One_When_Successful()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DEL1"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("DEL1");
            entity.IsDeleted = IsDelete.Deleted;
            var result = await CreateRepo(ctx).DeleteAsync(id, entity);

            result.Should().Be(1);
        }

        [Fact]
        public async Task DeleteAsync_Should_Flag_IsDeleted()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DEL2"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("DEL2");
            entity.IsDeleted = IsDelete.Deleted;
            await CreateRepo(ctx).DeleteAsync(id, entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.ItemGroup.FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity("GH");
            entity.IsDeleted = IsDelete.Deleted;

            var result = await CreateRepo(ctx).DeleteAsync(9999999, entity);

            result.Should().Be(-1);
        }

        // --- ExistsByCodeAsync ---

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Code_Exists()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(BuildEntity("EXCODE1"));

            var result = await CreateRepo(ctx).ExistsByCodeAsync("EXCODE1");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_When_Missing()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).ExistsByCodeAsync("NOCODE");

            result.Should().BeFalse();
        }

        // --- IsNameDuplicateAsync ---

        [Fact]
        public async Task IsNameDuplicateAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(BuildEntity("DNM1", "DupName"));

            var result = await CreateRepo(ctx).IsNameDuplicateAsync("DupName", 0);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsNameDuplicateAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DNM2", "SelfName"));

            var result = await CreateRepo(ctx).IsNameDuplicateAsync("SelfName", id);

            result.Should().BeFalse();
        }

        // --- IsCodeDuplicateAsync ---

        [Fact]
        public async Task IsCodeDuplicateAsync_Should_Return_True_For_Duplicate_Code()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(BuildEntity("CDUP1", "A"));

            var result = await CreateRepo(ctx).IsCodeDuplicateAsync("CDUP1", 0);

            result.Should().BeTrue();
        }

        // --- ExistsByNameAsync ---

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_True_For_Existing()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(BuildEntity("ENM1", "ExistsName"));

            var result = await CreateRepo(ctx).ExistsByNameAsync("ExistsName");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_False_When_Missing()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).ExistsByNameAsync("NoSuchName");

            result.Should().BeFalse();
        }
    }
}
