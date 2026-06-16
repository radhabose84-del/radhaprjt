using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.TaxCode;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.TaxCode
{
    [Collection("DatabaseCollection")]
    public sealed class TaxCodeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TaxCodeQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private TaxCodeQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private static TaxCodeCommandRepository CreateCommandRepo(ApplicationDbContext ctx) => new(ctx);

        // Seeds (or reuses) a MiscMaster value under a misc-type and returns its id.
        private async Task<int> SeedMiscAsync(string typeCode, string valueCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == typeCode);
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster { MiscTypeCode = typeCode, Description = typeCode, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var val = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.MiscTypeId == type.Id && m.Code == valueCode);
            if (val == null)
            {
                val = new Domain.Entities.MiscMaster { MiscTypeId = type.Id, Code = valueCode, Description = valueCode, SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
                await ctx.MiscMaster.AddAsync(val);
                await ctx.SaveChangesAsync();
            }
            return val.Id;
        }

        private async Task<int> SeedTaxCodeAsync(
            string code = "GST-OUT-5", string name = "GST Output 5%", string taxType = "GST_OUT",
            decimal rate = 5.0m, bool active = true, DateOnly? from = null)
        {
            var taxTypeId = await SeedMiscAsync("TAX_TYPE", taxType);
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateCommandRepo(ctx);
            var id = await repo.CreateTaxCodeAsync(new Domain.Entities.TaxCodeMaster
            {
                CompanyId = 1,
                TaxCode = code,
                TaxName = name,
                TaxTypeId = taxTypeId,
                IsActive = active ? Status.Active : Status.Inactive
            });
            await repo.CreateRateVersionAsync(new Domain.Entities.TaxCodeRateVersion
            {
                TaxCodeId = id,
                RatePercent = rate,
                EffectiveFrom = from ?? new DateOnly(2017, 7, 1)
            });
            return id;
        }

        [Fact]
        public async Task GetAllTaxCodesAsync_Should_Return_Seeded_With_CurrentRate()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedTaxCodeAsync();

            var (items, total) = await CreateQueryRepo().GetAllTaxCodesAsync(1, 10, null, null, null);

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items[0].CurrentRatePercent.Should().Be(5.0m);
        }

        [Fact]
        public async Task GetAllTaxCodesAsync_Should_Filter_By_TaxType()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedTaxCodeAsync(code: "GST-OUT-5", taxType: "GST_OUT");
            await SeedTaxCodeAsync(code: "GST-IN-5", taxType: "GST_IN", name: "GST Input 5%");

            var (items, _) = await CreateQueryRepo().GetAllTaxCodesAsync(1, 10, null, null, "GST_IN");

            items.Should().OnlyContain(x => x.TaxType == "GST_IN");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedTaxCodeAsync(code: "GST-IN-12", name: "GST Input 12%", taxType: "GST_IN", rate: 12.0m);

            var dto = await CreateQueryRepo().GetTaxCodeByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.TaxCode.Should().Be("GST-IN-12");
            dto.CurrentRatePercent.Should().Be(12.0m);
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedTaxCodeAsync(code: "GST-OUT-5");

            var exists = await CreateQueryRepo().TaxCodeAlreadyExistsAsync("GST-OUT-5", 1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            await _fixture.ClearAllTablesAsync();

            var notFound = await CreateQueryRepo().TaxCodeNotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedTaxCodeAsync(code: "GST-OUT-5", active: false);

            var results = await CreateQueryRepo().TaxCodeAutocompleteAsync("GST", null, null, CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task GetRateVersionsAsync_Should_Return_AllVersions_Ordered()
        {
            await _fixture.ClearAllTablesAsync();
            var id = await SeedTaxCodeAsync();
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await CreateCommandRepo(ctx).CreateRateVersionAsync(new Domain.Entities.TaxCodeRateVersion
                {
                    TaxCodeId = id,
                    RatePercent = 12.0m,
                    EffectiveFrom = new DateOnly(2026, 7, 1)
                });
            }

            var versions = await CreateQueryRepo().GetRateVersionsAsync(id);

            versions.Should().HaveCount(2);
            versions[0].VersionNo.Should().Be(1);
            versions[1].VersionNo.Should().Be(2);
        }

    }
}
