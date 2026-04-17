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
    public sealed class PriceGroupMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PriceGroupMasterLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PriceGroupMasterLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string code = "PGL1", Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var p = new InventoryManagement.Domain.Entities.PriceGroupMaster
            {
                PriceGroupCode = code,
                PriceGroupName = $"Name {code}",
                Description = "d",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.PriceGroupMaster.AddAsync(p);
            await ctx.SaveChangesAsync();
            return p.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllPriceGroupMasterAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("PGL-A");

            var result = await CreateRepo().GetAllPriceGroupMasterAsync();

            result.Should().Contain(p => p.PriceGroupCode == "PGL-A");
        }

        [Fact]
        public async Task GetAllPriceGroupMasterAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAsync("INACT", active: Status.Inactive);

            var result = await CreateRepo().GetAllPriceGroupMasterAsync();

            result.Should().NotContain(p => p.PriceGroupCode == "INACT");
        }

        [Fact]
        public async Task GetAllPriceGroupMasterAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("DEL", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetAllPriceGroupMasterAsync();

            result.Should().NotContain(p => p.PriceGroupCode == "DEL");
        }

        [Fact]
        public async Task GetAllPriceGroupMasterAsync_Should_Order_By_Code()
        {
            await ClearAsync();
            await SeedAsync("ZZZ");
            await SeedAsync("AAA");
            await SeedAsync("MMM");

            var result = await CreateRepo().GetAllPriceGroupMasterAsync();
            var codes = result.Where(p => p.PriceGroupCode is "ZZZ" or "AAA" or "MMM").Select(p => p.PriceGroupCode).ToList();

            codes.IndexOf("AAA").Should().BeLessThan(codes.IndexOf("MMM"));
            codes.IndexOf("MMM").Should().BeLessThan(codes.IndexOf("ZZZ"));
        }
    }
}
