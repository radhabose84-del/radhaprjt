using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FixedAssetManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class AssetGroupLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AssetGroupLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AssetGroupLookupRepository CreateLookupRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AssetGroupLookupRepository(conn);
        }

        private async Task<int> SeedAssetGroupAsync(
            string code = "AGL1",
            string name = "Group A",
            BaseEntity.Status active = BaseEntity.Status.Active,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var group = new FAM.Domain.Entities.AssetGroup
            {
                Code = code,
                GroupName = name,
                GroupPercentage = 10m,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.AssetGroup.AddAsync(group);
            await ctx.SaveChangesAsync();
            return group.Id;
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Groups()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedAssetGroupAsync("AGL_A", "Alpha");
            var id2 = await SeedAssetGroupAsync("AGL_B", "Beta");
            await SeedAssetGroupAsync("AGL_C", "Gamma");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(2);
            results.Select(r => r.Code).Should().Contain(new[] { "AGL_A", "AGL_B" });
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Map_Columns_Correctly()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedAssetGroupAsync("AGL_M", "Mapped");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id });

            results.Should().HaveCount(1);
            var dto = results[0];
            dto.AssetGroupId.Should().Be(id);
            dto.Code.Should().Be("AGL_M");
            dto.GroupName.Should().Be("Mapped");
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var results = await CreateLookupRepo().GetByIdsAsync(Array.Empty<int>());

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Null_Input()
        {
            var results = await CreateLookupRepo().GetByIdsAsync(null!);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Ignore_NonPositive_Ids()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedAssetGroupAsync("AGL_Z", "Z");

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id, 0, -1 });

            results.Should().HaveCount(1);
            results[0].AssetGroupId.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Deduplicate_Input_Ids()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedAssetGroupAsync();

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id, id, id });

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedAssetGroupAsync("AGL_AA", "Active");
            var id2 = await SeedAssetGroupAsync("AGL_II", "Inactive", active: BaseEntity.Status.Inactive);

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(1);
            results[0].AssetGroupId.Should().Be(id1);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedAssetGroupAsync("AGL_KK", "Keep");
            var id2 = await SeedAssetGroupAsync("AGL_DD", "Deleted", deleted: BaseEntity.IsDelete.Deleted);

            var results = await CreateLookupRepo().GetByIdsAsync(new[] { id1, id2 });

            results.Should().HaveCount(1);
            results[0].AssetGroupId.Should().Be(id1);
        }
    }
}
