using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Repositories.Lookups.Production;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class CountMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;
        public CountMasterLookupRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CountMasterLookupRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureMiscIdAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "CM_TYP");
            if (t == null)
            {
                t = new ProductionManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "CM_TYP",
                    Description = "CM Type",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscTypeMaster.Add(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "CM_MM");
            if (m == null)
            {
                m = new ProductionManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id,
                    Code = "CM_MM",
                    Description = "CM",
                    SortOrder = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscMaster.Add(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> SeedAsync(string code, string desc, bool isActive = true, bool isDeleted = false)
        {
            var miscId = await EnsureMiscIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new ProductionManagement.Domain.Entities.CountMaster
            {
                CountCode = code,
                CountValue = 30m,
                ShortName = "S",
                CountTypeId = miscId,
                CountCategoryId = miscId,
                CountDescription = desc,
                UOMId = 1,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = isDeleted ? IsDelete.Deleted : IsDelete.NotDeleted
            };
            ctx.CountMaster.Add(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        [Fact]
        public async Task GetAllAsync_Returns_Ordered_By_Description()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("C1", "Zeta");
            await SeedAsync("C2", "Alpha");

            var result = await CreateRepo().GetAllAsync();

            result.Select(r => r.CountDescription).Should().ContainInOrder("Alpha", "Zeta");
        }

        [Fact]
        public async Task GetAllAsync_Excludes_Inactive_And_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedAsync("C1", "Keep");
            await SeedAsync("C2", "Off", isActive: false);
            await SeedAsync("C3", "Gone", isDeleted: true);

            var result = await CreateRepo().GetAllAsync();

            result.Should().ContainSingle().Which.CountDescription.Should().Be("Keep");
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
            var id1 = await SeedAsync("C1", "A");
            var id2 = await SeedAsync("C2", "B");
            await SeedAsync("C3", "C");

            var result = await CreateRepo().GetByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
            result.Select(r => r.CountCode).Should().BeEquivalentTo(new[] { "C1", "C2" });
        }

        [Fact]
        public async Task GetByIdsAsync_Excludes_SoftDeleted()
        {
            await _fixture.ClearAllTablesAsync();
            var keep = await SeedAsync("C1", "K");
            var gone = await SeedAsync("C2", "G", isDeleted: true);

            var result = await CreateRepo().GetByIdsAsync(new[] { keep, gone });

            result.Should().ContainSingle().Which.Id.Should().Be(keep);
        }
    }
}
