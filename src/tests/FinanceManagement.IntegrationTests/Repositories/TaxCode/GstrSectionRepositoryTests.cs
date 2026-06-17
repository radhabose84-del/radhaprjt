using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.TaxCode;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.TaxCode
{
    [Collection("DatabaseCollection")]
    public sealed class GstrSectionRepositoryTests
    {
        private readonly DbFixture _fixture;

        public GstrSectionRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private GstrSectionQueryRepository CreateQueryRepo() => new(new SqlConnection(_fixture.ConnectionString));
        private static GstrSectionCommandRepository CreateCommandRepo(ApplicationDbContext ctx) => new(ctx);

        private async Task<int> SeedReportTypeAsync(string code = "GSTR-1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "GSTR_REPORT");
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster { MiscTypeCode = "GSTR_REPORT", Description = "GSTR Report", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }
            var val = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.MiscTypeId == type.Id && m.Code == code);
            if (val == null)
            {
                val = new Domain.Entities.MiscMaster { MiscTypeId = type.Id, Code = code, Description = code, SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted };
                await ctx.MiscMaster.AddAsync(val);
                await ctx.SaveChangesAsync();
            }
            return val.Id;
        }

        private async Task<int> SeedSectionAsync(int reportTypeId, string code = "4A", string name = "Taxable outward supplies (B2B)")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateSectionAsync(new Domain.Entities.GstrSectionMaster
            {
                CompanyId = 1,
                ReportTypeId = reportTypeId,
                SectionCode = code,
                SectionName = name,
                IsActive = Status.Active
            });
        }

        private async Task<int> SeedLinkageAsync(int sectionId, decimal? derived, decimal expected, decimal tolerance = 1m)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateLinkageAsync(new Domain.Entities.GstrSectionAccountLinkage
            {
                SectionMasterId = sectionId,
                AccountRangeFrom = "6100101",
                AccountRangeTo = "6100199",
                DerivedValue = derived,
                ExpectedValue = expected,
                TolerancePercent = tolerance,
                IsActive = Status.Active
            });
        }

        [Fact]
        public async Task GetAllSectionsAsync_Should_Return_Seeded_With_ReportType()
        {
            await _fixture.ClearAllTablesAsync();
            var rt = await SeedReportTypeAsync("GSTR-1");
            await SeedSectionAsync(rt, "4A", "Taxable outward supplies (B2B)");

            var (items, total) = await CreateQueryRepo().GetAllSectionsAsync(1, 10, null, 1, null);

            total.Should().Be(1);
            items[0].ReportType.Should().Be("GSTR-1");
            items[0].SectionCode.Should().Be("4A");
        }

        [Fact]
        public async Task SectionAlreadyExistsAsync_Should_Detect_Duplicate_Per_Report()
        {
            await _fixture.ClearAllTablesAsync();
            var rt = await SeedReportTypeAsync("GSTR-1");
            await SeedSectionAsync(rt, "4A");

            (await CreateQueryRepo().SectionAlreadyExistsAsync(1, rt, "4A")).Should().BeTrue();
            (await CreateQueryRepo().SectionAlreadyExistsAsync(1, rt, "5A")).Should().BeFalse();
        }

        [Fact]
        public async Task SectionHasLinkagesAsync_Should_Block_Delete_When_Mapped()
        {
            await _fixture.ClearAllTablesAsync();
            var rt = await SeedReportTypeAsync("GSTR-1");
            var sectionId = await SeedSectionAsync(rt);
            await SeedLinkageAsync(sectionId, derived: 100, expected: 100);

            (await CreateQueryRepo().SectionHasLinkagesAsync(sectionId)).Should().BeTrue();
        }

        [Fact]
        public async Task GetAllLinkagesAsync_Should_Compute_Tolerance_Verdict()
        {
            await _fixture.ClearAllTablesAsync();
            var rt = await SeedReportTypeAsync("GSTR-1");
            var sectionId = await SeedSectionAsync(rt);

            await SeedLinkageAsync(sectionId, derived: 8_400_000m, expected: 8_400_000m, tolerance: 1m); // exact → within
            await SeedLinkageAsync(sectionId, derived: 980_000m, expected: 1_000_000m, tolerance: 1m);   // 2% off → out

            var (rows, total) = await CreateQueryRepo().GetAllLinkagesAsync(1, 50, null, 1);

            total.Should().Be(2);
            rows.Single(r => r.ExpectedValue == 8_400_000m).WithinTolerance.Should().BeTrue();
            rows.Single(r => r.ExpectedValue == 1_000_000m).WithinTolerance.Should().BeFalse();
            rows.Single(r => r.ExpectedValue == 1_000_000m).ToleranceStatus.Should().Be("Out of tolerance");
        }
    }
}
