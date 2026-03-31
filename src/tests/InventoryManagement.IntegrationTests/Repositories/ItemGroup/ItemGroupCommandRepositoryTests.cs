using Microsoft.EntityFrameworkCore;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.Item.ItemGroup;

namespace InventoryManagement.IntegrationTests.Repositories.ItemGroup
{
    [Collection("DatabaseCollection")]
    public sealed class ItemGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ItemGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ItemGroupCommandRepository CreateRepository(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object);

        private static InventoryManagement.Domain.Entities.Item.ItemGroup BuildEntity(
            string code = "IG001",
            string name = "Test Item Group") =>
            new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                ItemGroupCode = code,
                ItemGroupName = name,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(InventoryManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Inventory].[ItemGroup]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("IG_ELEC", "Electronics"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ItemGroupCode.Should().Be("IG_ELEC");
            saved.ItemGroupName.Should().Be("Electronics");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ItemGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_Value_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("IG_UPD001", "Original Group"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(newId, new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                Id = newId,
                ItemGroupCode = "IG_UPD001",
                ItemGroupName = "Updated Group",
                IsActive = BaseEntity.Status.Active
            });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("IG_UPD002", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                Id = newId,
                ItemGroupCode = "IG_UPD002",
                ItemGroupName = "Updated Name",
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.ItemGroup.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.ItemGroupName.Should().Be("Updated Name");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Negative_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new InventoryManagement.Domain.Entities.Item.ItemGroup
            {
                Id = 9999,
                ItemGroupCode = "NOTFOUND",
                ItemGroupName = "Not Found"
            });

            result.Should().BeLessThan(0);
        }

        // --- DELETE (soft delete) ---

        [Fact]
        public async Task DeleteAsync_Should_Return_Value_GreaterThanZero_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new InventoryManagement.Domain.Entities.Item.ItemGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity("IG_DEL001", "Delete Me"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new InventoryManagement.Domain.Entities.Item.ItemGroup { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.ItemGroup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
