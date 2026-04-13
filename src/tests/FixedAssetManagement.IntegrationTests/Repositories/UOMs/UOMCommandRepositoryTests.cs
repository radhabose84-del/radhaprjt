using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;
using FAM.Infrastructure.Repositories.UOMs;

namespace FixedAssetManagement.IntegrationTests.Repositories.UOMs
{
    [Collection("DatabaseCollection")]
    public sealed class UOMCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UOMCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UOMCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscMasterAsync(string typeCode = "UOM_T")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode,
                Description = "UOM Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            return (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = "MM_" + typeCode,
                Description = "UOM Type Misc",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
        }

        private static UOM BuildEntity(int uomTypeId, string code = "UOM001", string name = "PCS") =>
            new UOM
            {
                Code = code,
                UOMName = name,
                UOMTypeId = uomTypeId,
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
            var typeId = await SeedMiscMasterAsync("UOM_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedMiscMasterAsync("UOM_C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "UOM_P", "Kilograms"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.UOMs.FirstAsync(x => x.Id == result.Id);
            saved.Code.Should().Be("UOM_P");
            saved.UOMName.Should().Be("Kilograms");
            saved.UOMTypeId.Should().Be(typeId);
        }

        [Fact]
        public async Task CreateAsync_Should_AutoGenerate_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedMiscMasterAsync("UOM_C3");

            var first = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "UOM_S1"));
            var second = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "UOM_S2"));

            second.SortOrder.Should().Be(first.SortOrder + 1);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedMiscMasterAsync("UOM_U1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "UOM_U", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new UOM
            {
                Id = created.Id,
                Code = "UOM_U",
                UOMName = "Renamed",
                SortOrder = created.SortOrder,
                UOMTypeId = typeId,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            (await ctx.UOMs.FirstAsync(x => x.Id == created.Id)).UOMName.Should().Be("Renamed");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new UOM { Id = 9999, UOMName = "X" });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedMiscMasterAsync("UOM_D1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id, new UOM { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.UOMs.IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, new UOM { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CheckForDuplicatesAsync_Should_Detect_Name_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedMiscMasterAsync("UOM_DUP");
            await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "UOM_DUP", "DupName"));

            var (nameDup, _) = await CreateRepository(ctx).CheckForDuplicatesAsync("DupName", 999, 0);

            nameDup.Should().BeTrue();
        }
    }
}
