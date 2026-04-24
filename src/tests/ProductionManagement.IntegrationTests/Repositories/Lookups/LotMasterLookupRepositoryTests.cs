using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.Lookups.Production;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class LotMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;
        public LotMasterLookupRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private LotMasterLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<(int LotTypeId, int StatusId)> EnsureMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "LM_TYP");
            if (t == null)
            {
                t = new ProductionManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "LM_TYP",
                    Description = "LotType",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscTypeMaster.Add(t);
                await ctx.SaveChangesAsync();
            }
            var lotType = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "LM_LT" && x.MiscTypeId == t.Id);
            if (lotType == null)
            {
                lotType = new ProductionManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id,
                    Code = "LM_LT",
                    Description = "LT",
                    SortOrder = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscMaster.Add(lotType);
                await ctx.SaveChangesAsync();
            }
            var status = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "LM_ST" && x.MiscTypeId == t.Id);
            if (status == null)
            {
                status = new ProductionManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id,
                    Code = "LM_ST",
                    Description = "ST",
                    SortOrder = 2,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscMaster.Add(status);
                await ctx.SaveChangesAsync();
            }
            return (lotType.Id, status.Id);
        }

        private async Task<int> SeedAsync(
            string code,
            int itemId = 1,
            bool isActive = true,
            bool isDeleted = false)
        {
            var (lotTypeId, statusId) = await EnsureMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new ProductionManagement.Domain.Entities.LotMaster
            {
                LotCode = code,
                BatchNumber = code + "_B",
                LotTypeId = lotTypeId,
                StatusId = statusId,
                ItemId = itemId,
                UnitId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                TotalProducedQty = 100m,
                AvailableQty = 100m,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.LotMaster.Add(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        [Fact]
        public async Task GetAllAsync_Returns_Ordered_By_LotCode()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("LOT_Z");
            await SeedAsync("LOT_A");

            var result = await CreateRepo().GetAllAsync();

            result.Select(r => r.LotCode).Should().ContainInOrder("LOT_A", "LOT_Z");
        }

        [Fact]
        public async Task GetAllAsync_Excludes_Inactive_And_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("LOT_KEEP");
            await SeedAsync("LOT_OFF", isActive: false);
            await SeedAsync("LOT_GONE", isDeleted: true);

            var result = await CreateRepo().GetAllAsync();

            result.Should().ContainSingle().Which.LotCode.Should().Be("LOT_KEEP");
        }

        [Fact]
        public async Task GetAllAsync_Returns_Empty_When_NoData()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().GetAllAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Empty_Input_Returns_Empty()
        {
            var result = await CreateRepo().GetByIdsAsync(Array.Empty<int>());
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Null_Input_Returns_Empty()
        {
            var result = await CreateRepo().GetByIdsAsync(null!);
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetByIdsAsync_Returns_Matching_Records()
        {
            await _fixture.ClearAllTablesAsync();
            var id1 = await SeedAsync("L1");
            var id2 = await SeedAsync("L2");
            await SeedAsync("L3");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.LotCode).Should().BeEquivalentTo(new[] { "L1", "L2" });
        }

        [Fact]
        public async Task GetByIdsAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var keep = await SeedAsync("L_K");
            var gone = await SeedAsync("L_G", isDeleted: true);

            var result = await CreateRepo().GetByIdsAsync(new[] { keep, gone });

            result.Should().ContainSingle().Which.Id.Should().Be(keep);
        }
    }
}
