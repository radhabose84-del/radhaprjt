using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.SpecificationMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.SpecificationMaster
{
    [Collection("DatabaseCollection")]
    public sealed class SpecificationMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SpecificationMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SpecificationMasterCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedAssetGroupAsync(string code = "SM_G1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = "Group " + code,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private static SpecificationMasters BuildEntity(int groupId, string name = "TestSpec") =>
            new SpecificationMasters
            {
                SpecificationName = name,
                AssetGroupId = groupId,
                ISDefault = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("SM_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("SM_C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "Length"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SpecificationMasters.FirstAsync(x => x.Id == result.Id);
            saved.SpecificationName.Should().Be("Length");
            saved.AssetGroupId.Should().Be(groupId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("SM_U1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new SpecificationMasters
            {
                Id = created.Id,
                SpecificationName = "Renamed",
                AssetGroupId = groupId,
                ISDefault = 1,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SpecificationMasters.FirstAsync(x => x.Id == created.Id);
            saved.SpecificationName.Should().Be("Renamed");
            saved.ISDefault.Should().Be((byte)1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new SpecificationMasters { Id = 9999, SpecificationName = "X" });

            result.Should().Be(0);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("SM_D1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id,
                new SpecificationMasters { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.SpecificationMasters.IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999,
                new SpecificationMasters { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(0);
        }

        [Fact]
        public async Task ExistsByAssetGroupIdAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("SM_E1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, "ExistName"));

            (await CreateRepository(ctx).ExistsByAssetGroupIdAsync(groupId, "ExistName")).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByAssetGroupIdAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("SM_E2");

            (await CreateRepository(ctx).ExistsByAssetGroupIdAsync(groupId, "NoSuch")).Should().BeFalse();
        }
    }
}
