using LogisticsManagement.Infrastructure.Repositories.Lookups.Logistics;
using Microsoft.Data.SqlClient;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class FreightMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FreightMasterLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private FreightMasterLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new FreightMasterLookupRepository(conn);
        }

        private async Task<int> SeedMiscAsync(string description)
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var miscType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "FreightMode");
            if (miscType == null)
            {
                miscType = new LogisticsManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "FreightMode",
                    Description = "Freight Mode",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscTypeMaster.Add(miscType);
                await ctx.SaveChangesAsync();
            }

            var misc = new LogisticsManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = description.Substring(0, Math.Min(description.Length, 10)),
                Description = description,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task<int> SeedFreightMasterAsync(
            int freightModeId, int rateMethodId, decimal rate = 100m, int moduleId = 1,
            bool isActive = true, bool isDeleted = false)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var freight = new LogisticsManagement.Domain.Entities.FreightMaster
            {
                FreightModeId = freightModeId,
                RateMethodId = rateMethodId,
                Rate = rate,
                ModuleId = moduleId,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.FreightMaster.Add(freight);
            await ctx.SaveChangesAsync();
            return freight.Id;
        }

        [Fact]
        public async Task GetAllFreightMasterAsync_Returns_Active_NonDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var modeId = await SeedMiscAsync("Road");
            var methodId = await SeedMiscAsync("Per-Km");
            await SeedFreightMasterAsync(modeId, methodId, rate: 50m);

            var result = await CreateRepo().GetAllFreightMasterAsync();

            result.Should().ContainSingle();
            result[0].Rate.Should().Be(50m);
            result[0].FreightModeName.Should().Be("Road");
            result[0].RateMethodName.Should().Be("Per-Km");
        }

        [Fact]
        public async Task GetAllFreightMasterAsync_Excludes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            var modeId = await SeedMiscAsync("Road");
            var methodId = await SeedMiscAsync("Per-Km");
            await SeedFreightMasterAsync(modeId, methodId, isActive: false);

            var result = await CreateRepo().GetAllFreightMasterAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllFreightMasterAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var modeId = await SeedMiscAsync("Road");
            var methodId = await SeedMiscAsync("Per-Km");
            await SeedFreightMasterAsync(modeId, methodId, isDeleted: true);

            var result = await CreateRepo().GetAllFreightMasterAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllFreightMasterAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllFreightMasterAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByModuleIdAsync_Filters_By_ModuleId()
        {
            await _fixture.ClearAllTablesAsync();
            // Unique index on (FreightModeId, RateMethodId) - must use different combos per row
            var mode1 = await SeedMiscAsync("Road");
            var method1 = await SeedMiscAsync("Per-Km");
            var mode2 = await SeedMiscAsync("Rail");
            var method2 = await SeedMiscAsync("Per-Ton");
            await SeedFreightMasterAsync(mode1, method1, moduleId: 1);
            await SeedFreightMasterAsync(mode2, method2, moduleId: 2);

            var result = await CreateRepo().GetByModuleIdAsync(1);

            result.Should().ContainSingle().Which.ModuleId.Should().Be(1);
        }

        [Fact]
        public async Task GetByModuleIdAsync_Returns_Empty_When_NoMatch()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetByModuleIdAsync(999);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Matching_Row()
        {
            await _fixture.ClearAllTablesAsync();
            var modeId = await SeedMiscAsync("Road");
            var methodId = await SeedMiscAsync("Per-Km");
            var id = await SeedFreightMasterAsync(modeId, methodId, rate: 99m);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Rate.Should().Be(99m);
        }

        [Fact]
        public async Task GetByIdAsync_Returns_Null_When_Not_Found()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Excludes_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            var modeId = await SeedMiscAsync("Road");
            var methodId = await SeedMiscAsync("Per-Km");
            var id = await SeedFreightMasterAsync(modeId, methodId, isActive: false);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }
    }
}
