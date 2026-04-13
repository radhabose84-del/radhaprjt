using Dapper;
using Microsoft.Data.SqlClient;
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
    public sealed class AssetWarrantyQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetWarrantyQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetWarrantyQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetWarrantyQueryRepository(conn);
        }

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
                AssetCode = codePrefix + "_AM", AssetName = "Test " + codePrefix,
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

        private async Task<int> SeedWarrantyAsync(int assetId, int warrantyTypeId, string contact = "ContactQ")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new AssetWarrantyCommandRepository(ctx).CreateAsync(new AssetWarranties
            {
                AssetId = assetId,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2026, 1, 1),
                Period = 12,
                WarrantyType = warrantyTypeId,
                WarrantyProvider = "Test Provider",
                ContactPerson = contact,
                MobileNumber = "1234567890",
                ServiceCountryId = 1,
                ServiceStateId = 1,
                ServiceCityId = 1,
                ServiceMobileNumber = "9876543210",
                ServiceEmail = "service@example.com",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetByAssetWarrantyNameAsync_Should_Return_Match()
        {
            await ClearTablesAsync();
            var wtId = await SeedMiscMasterAsync("AWQ_WT1");
            var assetId = await SeedAssetAsync("AWQ_1");
            await SeedWarrantyAsync(assetId, wtId);

            var result = await CreateQueryRepo().GetByAssetWarrantyNameAsync("Test");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Match_When_Found()
        {
            await ClearTablesAsync();
            var wtId = await SeedMiscMasterAsync("AWQ_WT2");
            var assetId = await SeedAssetAsync("AWQ_2");
            var warrantyId = await SeedWarrantyAsync(assetId, wtId);

            var result = await CreateQueryRepo().GetByIdAsync(warrantyId);

            result.Should().NotBeNull();
            result.AssetId.Should().Be(assetId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_KeyNotFoundException_When_NotFound()
        {
            await ClearTablesAsync();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => CreateQueryRepo().GetByIdAsync(9999));
        }

        [Fact]
        public async Task GetWarrantyTypeAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetWarrantyTypeAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetWarrantyClaimStatusAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetWarrantyClaimStatusAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_True_When_Linked_To_Existing_Asset()
        {
            await ClearTablesAsync();
            var wtId = await SeedMiscMasterAsync("AWQ_WT3");
            var assetId = await SeedAssetAsync("AWQ_3");
            var warrantyId = await SeedWarrantyAsync(assetId, wtId);

            (await CreateQueryRepo().SoftDeleteValidation(warrantyId)).Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_NotFound()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo().SoftDeleteValidation(9999)).Should().BeFalse();
        }

        [Fact]
        public async Task GetBaseDirectoryAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetBaseDirectoryAsync();

            result.Should().BeEmpty();
        }
    }
}
