using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.DepreciationGroup;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.DepreciationGroup
{
    [Collection("DatabaseCollection")]
    public sealed class DepreciationGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DepreciationGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DepreciationGroupQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DepreciationGroupQueryRepository(conn);
        }

        private async Task<int> SeedAssetGroupAsync(string code)
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

        private async Task<int> SeedEntityAsync(int groupId, int bookType, int method, string code = "DGQ_001", string name = "DG Q")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new DepreciationGroupCommandRepository(ctx).CreateAsync(new DepreciationGroups
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
            });
            return result.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllDepreciationGroupAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("DGQ_G1");
            var bookType = await SeedMiscMasterAsync("DGQ_BT1");
            var method = await SeedMiscMasterAsync("DGQ_M1");
            await SeedEntityAsync(groupId, bookType, method);

            var (items, total) = await CreateQueryRepo().GetAllDepreciationGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllDepreciationGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("DGQ_G2");
            var bookType = await SeedMiscMasterAsync("DGQ_BT2");
            var method = await SeedMiscMasterAsync("DGQ_M2");
            await SeedEntityAsync(groupId, bookType, method, "DGQ_A", "Alpha");
            await SeedEntityAsync(groupId, bookType, method, "DGQ_B", "Beta");

            var (items, _) = await CreateQueryRepo().GetAllDepreciationGroupAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].DepreciationGroupName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("DGQ_G3");
            var bookType = await SeedMiscMasterAsync("DGQ_BT3");
            var method = await SeedMiscMasterAsync("DGQ_M3");
            var id = await SeedEntityAsync(groupId, bookType, method, "DGQ_ID", "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Code.Should().Be("DGQ_ID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_KeyNotFoundException_When_NotFound()
        {
            await ClearTablesAsync();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => CreateQueryRepo().GetByIdAsync(9999));
        }

        [Fact]
        public async Task GetByDepreciationNameAsync_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("DGQ_G4");
            var bookType = await SeedMiscMasterAsync("DGQ_BT4");
            var method = await SeedMiscMasterAsync("DGQ_M4");
            await SeedEntityAsync(groupId, bookType, method, "DGQ_AC", "AutoDG");

            var result = await CreateQueryRepo().GetByDepreciationNameAsync("Auto");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetDepreciationMethodAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetDepreciationMethodAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetBookTypeAsync_Should_Return_Empty_When_No_MiscType_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetBookTypeAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_No_AssetMaster_Linked()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("DGQ_G5");
            var bookType = await SeedMiscMasterAsync("DGQ_BT5");
            var method = await SeedMiscMasterAsync("DGQ_M5");
            var id = await SeedEntityAsync(groupId, bookType, method);

            (await CreateQueryRepo().SoftDeleteValidation(id)).Should().BeFalse();
        }
    }
}
