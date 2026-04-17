using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities;
using InventoryManagement.Infrastructure.Repositories.Lookups;
using InventoryManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class HSNLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public HSNLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private HSNLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new HSNLookupRepository(conn);
        }

        private async Task<int> SeedMiscTypeAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "HSN_TYPE");
            if (existing != null) return existing.Id;

            var type = new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "HSN_TYPE", Description = "HSN Type",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(type);
            await ctx.SaveChangesAsync();
            return type.Id;
        }

        private async Task<int> SeedMiscMasterAsync(string code = "GOODS")
        {
            var typeId = await SeedMiscTypeAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == code);
            if (existing != null) return existing.Id;

            var misc = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId, Code = code, Description = code, SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task<int> SeedHSNAsync(
            string code = "1234",
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            var miscId = await SeedMiscMasterAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var h = new HSNMaster
            {
                HSNCode = code,
                Description = $"HSN {code}",
                TypeId = miscId,
                GSTCategoryId = miscId,
                GSTPercentage = 18m,
                IGSTPercentage = 18m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.HSNMaster.AddAsync(h);
            await ctx.SaveChangesAsync();
            return h.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- GetAllAsync ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearAsync();
            await SeedHSNAsync("1001");

            var result = await CreateRepo().GetAllAsync();

            result.Should().Contain(h => h.HSNCode == "1001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedHSNAsync("INACT", active: Status.Inactive);

            var result = await CreateRepo().GetAllAsync();

            result.Should().NotContain(h => h.HSNCode == "INACT");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedHSNAsync("DEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetAllAsync();

            result.Should().NotContain(h => h.HSNCode == "DEL");
        }

        // --- GetByIdsAsync ---

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching_Records()
        {
            await ClearAsync();
            var id1 = await SeedHSNAsync("2001");
            var id2 = await SeedHSNAsync("2002");
            await SeedHSNAsync("2003");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Empty_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Empty_For_Null_Input()
        {
            var result = await CreateRepo().GetByIdsAsync(null!);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id1 = await SeedHSNAsync("K1");
            var id2 = await SeedHSNAsync("K2", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }
    }
}
