using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Domain.Entities.AssetMaster;
using FAM.Infrastructure.Repositories.AssetMaster.AssetLocation;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetLocation
{
    [Collection("DatabaseCollection")]
    public sealed class AssetLocationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetLocationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetLocationCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<(int assetId, int locationId, int subLocationId)> SeedAssetAndLocationsAsync(string codePrefix)
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

            var loc = new Location
            {
                Code = codePrefix + "_L", LocationName = "L", UnitId = 1, DepartmentId = 1,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.Locations.Add(loc);
            await ctx.SaveChangesAsync();

            var subLoc = new FAM.Domain.Entities.SubLocation
            {
                Code = codePrefix + "_SL", SubLocationName = "SL",
                UnitId = 1, DepartmentId = 1, LocationId = loc.Id,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            ctx.SubLocations.Add(subLoc);
            await ctx.SaveChangesAsync();

            return (asset.Id, loc.Id, subLoc.Id);
        }

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, locationId, subLocationId) = await SeedAssetAndLocationsAsync("ALOC_C1");

            var result = await CreateRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetLocation
            {
                AssetId = assetId,
                UnitId = 1,
                DepartmentId = 1,
                LocationId = locationId,
                SubLocationId = subLocationId,
                CustodianId = 0,
                UserID = 1
            });

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, locationId, subLocationId) = await SeedAssetAndLocationsAsync("ALOC_C2");

            var result = await CreateRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetLocation
            {
                AssetId = assetId,
                UnitId = 1,
                DepartmentId = 5,
                LocationId = locationId,
                SubLocationId = subLocationId,
                CustodianId = 7,
                UserID = 1
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AssetLocations.FirstAsync(x => x.Id == result.Id);
            saved.AssetId.Should().Be(assetId);
            saved.DepartmentId.Should().Be(5);
            saved.CustodianId.Should().Be(7);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var (assetId, locationId, subLocationId) = await SeedAssetAndLocationsAsync("ALOC_U1");

            await CreateRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetLocation
            {
                AssetId = assetId,
                UnitId = 1, DepartmentId = 1,
                LocationId = locationId, SubLocationId = subLocationId,
                CustodianId = 1, UserID = 1
            });
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).UpdateAsync(assetId, new FAM.Domain.Entities.AssetMaster.AssetLocation
            {
                UnitId = 1, DepartmentId = 9,
                LocationId = locationId, SubLocationId = subLocationId,
                CustodianId = 99, UserID = 1
            });
            ctx.ChangeTracker.Clear();

            result.Should().BeGreaterThan(0);
            var updated = await ctx.AssetLocations.FirstAsync(x => x.AssetId == assetId);
            updated.DepartmentId.Should().Be(9);
            updated.CustodianId.Should().Be(99);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(9999, new FAM.Domain.Entities.AssetMaster.AssetLocation());

            result.Should().Be(0);
        }
    }
}
