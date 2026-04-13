using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.DepreciationGroup;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.DepreciationGroup
{
    [Collection("DatabaseCollection")]
    public sealed class DepreciationGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepreciationGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DepreciationGroupCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedAssetGroupAsync(string code = "DG_AG1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = "G " + code,
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedMiscMasterAsync(string typeCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode,
                Description = "Misc Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            return (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = "MM_" + typeCode,
                Description = "Misc",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
        }

        private static DepreciationGroups BuildEntity(int groupId, int? bookType, int? method, string code = "DG001", string name = "Test DG") =>
            new DepreciationGroups
            {
                Code = code,
                DepreciationGroupName = name,
                BookType = bookType,
                AssetGroupId = groupId,
                UsefulLife = 10m,
                DepreciationMethod = method,
                ResidualValue = 5,
                DepreciationRate = 10.5m,
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
            var groupId = await SeedAssetGroupAsync("DG_C1");
            var bookType = await SeedMiscMasterAsync("DG_BT1");
            var method = await SeedMiscMasterAsync("DG_M1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, bookType, method));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DG_C2");
            var bookType = await SeedMiscMasterAsync("DG_BT2");
            var method = await SeedMiscMasterAsync("DG_M2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, bookType, method, "DG_P", "PersistMe"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DepreciationGroups.FirstAsync(x => x.Id == result.Id);
            saved.Code.Should().Be("DG_P");
            saved.DepreciationGroupName.Should().Be("PersistMe");
            saved.AssetGroupId.Should().Be(groupId);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DG_C3");
            var bookType = await SeedMiscMasterAsync("DG_BT3");
            var method = await SeedMiscMasterAsync("DG_M3");

            var first = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, bookType, method, "DG_S1"));
            var second = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, bookType, method, "DG_S2"));

            second.SortOrder.Should().Be(first.SortOrder + 1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DG_U1");
            var bookType = await SeedMiscMasterAsync("DG_BT4");
            var method = await SeedMiscMasterAsync("DG_M4");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, bookType, method, "DG_U", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new DepreciationGroups
            {
                Id = created.Id,
                Code = "DG_U",
                DepreciationGroupName = "Renamed",
                BookType = bookType,
                AssetGroupId = groupId,
                UsefulLife = 15m,
                DepreciationMethod = method,
                ResidualValue = 10,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            (await ctx.DepreciationGroups.FirstAsync(x => x.Id == created.Id)).DepreciationGroupName.Should().Be("Renamed");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new DepreciationGroups { Id = 9999, DepreciationGroupName = "X" });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DG_D1");
            var bookType = await SeedMiscMasterAsync("DG_BT5");
            var method = await SeedMiscMasterAsync("DG_M5");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, bookType, method));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id, new DepreciationGroups { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.DepreciationGroups.IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, new DepreciationGroups { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(0);
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DG_E1");
            var bookType = await SeedMiscMasterAsync("DG_BT6");
            var method = await SeedMiscMasterAsync("DG_M6");
            await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, bookType, method, "DG_EX"));

            (await CreateRepository(ctx).ExistsByCodeAsync("DG_EX")).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByAssetGroupIdAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DG_E2");

            (await CreateRepository(ctx).ExistsByAssetGroupIdAsync(groupId)).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByAssetGroupIdAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            (await CreateRepository(ctx).ExistsByAssetGroupIdAsync(9999)).Should().BeFalse();
        }

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Return_Match_When_Composite_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await SeedAssetGroupAsync("DG_DUP1");
            var bookType = await SeedMiscMasterAsync("DG_BT7");
            var method = await SeedMiscMasterAsync("DG_M7");
            await CreateRepository(ctx).CreateAsync(BuildEntity(groupId, bookType, method, "DG_DUP1"));

            var result = await CreateRepository(ctx).CheckForDuplicatesAsync(groupId, method!, bookType, 0);

            result.Should().NotBeNull();
        }
    }
}
