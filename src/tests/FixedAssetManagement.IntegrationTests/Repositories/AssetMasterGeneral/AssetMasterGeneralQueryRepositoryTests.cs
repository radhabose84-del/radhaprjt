using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetMaster.AssetMasterGeneral;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetMasterGeneral
{
    [Collection("DatabaseCollection")]
    public sealed class AssetMasterGeneralQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetMasterGeneralQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetMasterGeneralQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(1);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");

            var dept = new Mock<IDepartmentLookup>(MockBehavior.Loose);
            var unit = new Mock<IUnitLookup>(MockBehavior.Loose);
            var country = new Mock<ICountryLookup>(MockBehavior.Loose);
            var state = new Mock<IStateLookup>(MockBehavior.Loose);
            var city = new Mock<ICityLookup>(MockBehavior.Loose);
            var company = new Mock<ICompanyLookup>(MockBehavior.Loose);

            return new AssetMasterGeneralQueryRepository(conn, ipMock.Object,
                dept.Object, unit.Object, country.Object, state.Object, city.Object, company.Object);
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

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetWorkingStatusAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetWorkingStatusAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAssetTypeAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetAssetTypeAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetBaseDirectoryAsync_Should_Throw_When_StoredProc_Missing()
        {
            await ClearTablesAsync();

            // Stored procedure dbo.FAM_GetBaseDirectory does not exist in the test database.
            Func<Task> act = async () => await CreateQueryRepo().GetBaseDirectoryAsync();
            await act.Should().ThrowAsync<Microsoft.Data.SqlClient.SqlException>();
        }

        [Fact]
        public async Task GetDocumentDirectoryAsync_Should_Return_Empty_When_No_MiscType()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetDocumentDirectoryAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAssetChildDetails_Should_Return_False_When_No_Children()
        {
            await ClearTablesAsync();
            var assetId = await SeedAssetAsync("AMGQ_C1");

            (await CreateQueryRepo().GetAssetChildDetails(assetId)).Should().BeFalse();
        }

        [Fact]
        public async Task IsAssetMasterLinkedAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTablesAsync();
            var assetId = await SeedAssetAsync("AMGQ_L1");

            (await CreateQueryRepo().IsAssetMasterLinkedAsync(assetId)).Should().BeFalse();
        }

        [Fact]
        public async Task GetByParentIdAsync_Should_Throw_When_NotFound()
        {
            await ClearTablesAsync();

            Func<Task> act = async () => await CreateQueryRepo().GetByParentIdAsync(9999);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
    }
}
