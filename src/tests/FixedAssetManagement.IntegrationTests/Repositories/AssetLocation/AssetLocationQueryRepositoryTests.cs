using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetMaster.AssetLocation;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetLocation
{
    [Collection("DatabaseCollection")]
    public sealed class AssetLocationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetLocationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetLocationQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetLocationQueryRepository(conn);
        }

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

        private async Task<int> SeedAssetLocationAsync(int assetId, int locationId, int subLocationId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new AssetLocationCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetMaster.AssetLocation
            {
                AssetId = assetId,
                UnitId = 1,
                DepartmentId = 1,
                LocationId = locationId,
                SubLocationId = subLocationId,
                CustodianId = 0,
                UserID = 1
            });
            return result.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAssetLocationAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var (assetId, locId, subLocId) = await SeedAssetAndLocationsAsync("ALOCQ_1");
            await SeedAssetLocationAsync(assetId, locId, subLocId);

            var (items, total) = await CreateQueryRepo().GetAllAssetLocationAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetLocationAsync_Should_Return_Empty_When_NoData()
        {
            await ClearTablesAsync();

            var (items, total) = await CreateQueryRepo().GetAllAssetLocationAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Match()
        {
            await ClearTablesAsync();
            var (assetId, locId, subLocId) = await SeedAssetAndLocationsAsync("ALOCQ_2");
            await SeedAssetLocationAsync(assetId, locId, subLocId);

            var result = await CreateQueryRepo().GetByIdAsync(assetId);

            result.Should().NotBeNull();
            result.AssetId.Should().Be(assetId);
        }

        [Fact]
        public async Task GetSublocationByIdAsync_Should_Return_Children()
        {
            await ClearTablesAsync();
            var (_, locId, _) = await SeedAssetAndLocationsAsync("ALOCQ_3");

            var result = await CreateQueryRepo().GetSublocationByIdAsync(locId);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetSublocationByIdAsync_Should_Return_Empty_When_NoMatch()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetSublocationByIdAsync(9999);

            result.Should().BeEmpty();
        }
    }
}
