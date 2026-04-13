using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.SpecificationMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.SpecificationMaster
{
    [Collection("DatabaseCollection")]
    public sealed class SpecificationMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SpecificationMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SpecificationMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SpecificationMasterQueryRepository(conn);
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

        private async Task<int> SeedEntityAsync(int groupId, string name = "SpecQ")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new SpecificationMasterCommandRepository(ctx).CreateAsync(new SpecificationMasters
            {
                SpecificationName = name,
                AssetGroupId = groupId,
                ISDefault = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllSpecificationGroupAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("SMQ_G1");
            await SeedEntityAsync(groupId);

            var (items, total) = await CreateQueryRepo().GetAllSpecificationGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllSpecificationGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("SMQ_G2");
            await SeedEntityAsync(groupId, "Alpha");
            await SeedEntityAsync(groupId, "Beta");

            var (items, _) = await CreateQueryRepo().GetAllSpecificationGroupAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].SpecificationName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetAllSpecificationGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("SMQ_G3");
            var id = await SeedEntityAsync(groupId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new SpecificationMasterCommandRepository(ctx).DeleteAsync(id,
                new SpecificationMasters { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllSpecificationGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("SMQ_G4");
            var id = await SeedEntityAsync(groupId, "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.SpecificationName.Should().Be("ById");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_KeyNotFoundException_When_NotFound()
        {
            await ClearTablesAsync();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => CreateQueryRepo().GetByIdAsync(9999));
        }

        [Fact]
        public async Task GetBySpecificationNameAsync_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("SMQ_G5");
            await SeedEntityAsync(groupId, "AutoSpec");

            var result = await CreateQueryRepo().GetBySpecificationNameAsync(groupId, "Auto");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task SoftDeleteValidation_Should_Return_False_When_No_Dependents()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("SMQ_G6");
            var id = await SeedEntityAsync(groupId);

            (await CreateQueryRepo().SoftDeleteValidation(id)).Should().BeFalse();
        }

        [Fact]
        public async Task IsSpecificationMasterLinkedAsync_Should_Return_False_When_No_Children()
        {
            await ClearTablesAsync();
            var groupId = await SeedAssetGroupAsync("SMQ_G7");
            var id = await SeedEntityAsync(groupId);

            (await CreateQueryRepo().IsSpecificationMasterLinkedAsync(id)).Should().BeFalse();
        }
    }
}
