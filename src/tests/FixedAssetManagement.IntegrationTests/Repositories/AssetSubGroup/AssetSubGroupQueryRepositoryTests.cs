using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.AssetSubGroup;

namespace FixedAssetManagement.IntegrationTests.Repositories.AssetSubGroup
{
    [Collection("DatabaseCollection")]
    public sealed class AssetSubGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetSubGroupQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetSubGroupQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetSubGroupQueryRepository(conn);
        }

        private async Task<int> SeedGroupAsync(string code = "ASGQ_G1")
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

        private async Task<int> SeedEntityAsync(int groupId, string code = "ASGQ_S1", string name = "Sub Q")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new AssetSubGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetSubGroup
            {
                Code = code,
                SubGroupName = name,
                GroupId = groupId,
                SubGroupPercentage = 5m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAssetSubGroupAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var groupId = await SeedGroupAsync("ASGQ_G1");
            await SeedEntityAsync(groupId);

            var (items, total) = await CreateQueryRepo().GetAllAssetSubGroupAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAssetSubGroupAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var groupId = await SeedGroupAsync("ASGQ_G2");
            await SeedEntityAsync(groupId, "ASGQ_A", "Alpha");
            await SeedEntityAsync(groupId, "ASGQ_B", "Beta");

            var (items, _) = await CreateQueryRepo().GetAllAssetSubGroupAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].SubGroupName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetAllAssetSubGroupAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var groupId = await SeedGroupAsync("ASGQ_G3");
            var id = await SeedEntityAsync(groupId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new AssetSubGroupCommandRepository(ctx).DeleteAsync(id,
                new FAM.Domain.Entities.AssetSubGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            var (items, total) = await CreateQueryRepo().GetAllAssetSubGroupAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var groupId = await SeedGroupAsync("ASGQ_G4");
            var id = await SeedEntityAsync(groupId, "ASGQ_ID", "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("ASGQ_ID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByGroupIdAsync_Should_Return_Children()
        {
            await ClearTablesAsync();
            var groupId = await SeedGroupAsync("ASGQ_G5");
            await SeedEntityAsync(groupId, "ASGQ_C1");
            await SeedEntityAsync(groupId, "ASGQ_C2");

            var result = await CreateQueryRepo().GetByGroupIdAsync(groupId);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByGroupIdAsync_Should_Return_Empty_When_GroupId_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByGroupIdAsync(9999);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAssetSubGroups_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var groupId = await SeedGroupAsync("ASGQ_G6");
            await SeedEntityAsync(groupId, "ASGQ_AC", "AutoSubGroup");

            var result = await CreateQueryRepo().GetAssetSubGroups("Auto");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task IsAssetSubGroupLinkedAsync_Should_Return_False_When_No_Children()
        {
            await ClearTablesAsync();
            var groupId = await SeedGroupAsync("ASGQ_G7");
            var id = await SeedEntityAsync(groupId);

            (await CreateQueryRepo().IsAssetSubGroupLinkedAsync(id)).Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTablesAsync();
            var groupId = await SeedGroupAsync("ASGQ_G8");
            var id = await SeedEntityAsync(groupId);

            (await CreateQueryRepo().SoftDeleteValidationAsync(id)).Should().BeFalse();
        }
    }
}
