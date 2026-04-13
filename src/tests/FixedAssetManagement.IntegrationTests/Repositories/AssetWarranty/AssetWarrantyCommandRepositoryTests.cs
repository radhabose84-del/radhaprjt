using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetWarranty;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetWarranty
{
    [Collection("DatabaseCollection")]
    public sealed class AssetWarrantyCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetWarrantyCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetWarrantyCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscMasterAsync(string typeCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode,
                Description = "T",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            return (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = "MM_" + typeCode,
                Description = "M",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
        }

        private async Task<int> SeedAssetAsync(string codePrefix)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new FAM.Domain.Entities.AssetGroup
            {
                Code = codePrefix + "_G", GroupName = "G", GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetGroup.Add(group);
            await ctx.SaveChangesAsync();

            var cat = new FAM.Domain.Entities.AssetCategories
            {
                Code = codePrefix + "_C", CategoryName = "C", AssetGroupId = group.Id,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetCategories.Add(cat);
            await ctx.SaveChangesAsync();

            var sub = new FAM.Domain.Entities.AssetSubCategories
            {
                Code = codePrefix + "_SC", SubCategoryName = "SC", AssetCategoriesId = cat.Id,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetSubCategories.Add(sub);
            await ctx.SaveChangesAsync();

            var asset = new AssetMasterGenerals
            {
                CompanyId = 1, UnitId = 1,
                AssetCode = codePrefix + "_AM", AssetName = "Asset",
                AssetGroupId = group.Id,
                AssetCategoryId = cat.Id,
                AssetSubCategoryId = sub.Id,
                Quantity = 1,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.AssetMasterGenerals.Add(asset);
            await ctx.SaveChangesAsync();
            return asset.Id;
        }

        private static AssetWarranties BuildEntity(int assetId, int warrantyTypeId, string contactPerson = "Test Contact") =>
            new AssetWarranties
            {
                AssetId = assetId,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2026, 1, 1),
                Period = 12,
                WarrantyType = warrantyTypeId,
                WarrantyProvider = "Test Provider",
                Description = "Test warranty",
                ContactPerson = contactPerson,
                MobileNumber = "1234567890",
                Email = "warranty@example.com",
                ServiceCountryId = 1,
                ServiceStateId = 1,
                ServiceCityId = 1,
                ServiceMobileNumber = "9876543210",
                ServiceEmail = "service@example.com",
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
            var wtId = await SeedMiscMasterAsync("AW_WT_C1");
            var assetId = await SeedAssetAsync("AW_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, wtId));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var wtId = await SeedMiscMasterAsync("AW_WT_C2");
            var assetId = await SeedAssetAsync("AW_C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, wtId, "PersistContact"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetWarranties.FirstAsync(x => x.Id == result.Id);
            saved.ContactPerson.Should().Be("PersistContact");
            saved.AssetId.Should().Be(assetId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var wtId = await SeedMiscMasterAsync("AW_WT_U1");
            var assetId = await SeedAssetAsync("AW_U1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, wtId, "Original"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(new AssetWarranties
            {
                Id = created.Id,
                AssetId = assetId,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2026, 12, 31),
                Period = 24,
                WarrantyType = wtId,
                WarrantyProvider = "Updated Provider",
                Description = "Updated warranty",
                ContactPerson = "Renamed",
                MobileNumber = "9999999999",
                Email = "new@example.com",
                ServiceCountryId = 1,
                ServiceStateId = 1,
                ServiceCityId = 1,
                ServiceMobileNumber = "8888888888",
                ServiceEmail = "updated-service@example.com",
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            (await ctx.AssetWarranties.FirstAsync(x => x.Id == created.Id)).ContactPerson.Should().Be("Renamed");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new AssetWarranties { Id = 9999, AssetId = 1 });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var wtId = await SeedMiscMasterAsync("AW_WT_D1");
            var assetId = await SeedAssetAsync("AW_D1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, wtId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id, new AssetWarranties { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.AssetWarranties.IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, new AssetWarranties { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(0);
        }

        [Fact]
        public async Task ExistsByAssetIdAsync_Should_Return_True_When_Active_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var wtId = await SeedMiscMasterAsync("AW_WT_E1");
            var assetId = await SeedAssetAsync("AW_E1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(assetId, wtId));

            (await CreateRepository(ctx).ExistsByAssetIdAsync(assetId)).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByAssetIdAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            (await CreateRepository(ctx).ExistsByAssetIdAsync(9999)).Should().BeFalse();
        }
    }
}
