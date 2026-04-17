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
    public sealed class UOMLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UOMLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UOMLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureUomTypeMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(m => m.MiscTypeCode == "UOM_TYPE");
            if (miscType == null)
            {
                miscType = new InventoryManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "UOM_TYPE", Description = "UOM Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
            }

            var misc = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == "UOM_COUNT");
            if (misc != null) return misc.Id;
            misc = new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id, Code = "UOM_COUNT", Description = "Count", SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task<int> SeedUomAsync(string code = "KG", Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var typeId = await EnsureUomTypeMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var u = new UOM
            {
                Code = code, UOMName = code,
                UOMTypeId = typeId, SortOrder = 1,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.UOMs.AddAsync(u);
            await ctx.SaveChangesAsync();
            return u.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Uom()
        {
            await ClearAsync();
            await SeedUomAsync("KG");

            var result = await CreateRepo().GetAllAsync();

            result.Should().Contain(u => u.Code == "KG");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedUomAsync("INACT", active: Status.Inactive);

            var result = await CreateRepo().GetAllAsync();

            result.Should().NotContain(u => u.Code == "INACT");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedUomAsync("DEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetAllAsync();

            result.Should().NotContain(u => u.Code == "DEL");
        }

        [Fact]
        public async Task GetByIdsAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id1 = await SeedUomAsync("A");
            var id2 = await SeedUomAsync("B");
            await SeedUomAsync("C");

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
            var id1 = await SeedUomAsync("K1");
            var id2 = await SeedUomAsync("K2", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }
    }
}
