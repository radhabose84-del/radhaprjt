using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.FinancialYearMaster;
using FinanceManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.FinancialYearMaster
{
    [Collection("DatabaseCollection")]
    public sealed class FinancialYearMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public FinancialYearMasterCommandRepositoryTests(DbFixture fixture) { _fixture = fixture; }

        private FinancialYearMasterCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.FinancialYearMaster BuildYear(int statusId, int companyId = 1) =>
            new()
            {
                CompanyId = companyId,
                FinancialYearCode = "2024-25",
                StartDate = new DateOnly(2024, 4, 1),
                EndDate   = new DateOnly(2025, 3, 31),
                StatusId  = statusId,
                IsTransitionYear = false,
                IsActive  = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private static List<Domain.Entities.FinancialPeriodMaster> Build13Periods(
            DateOnly start, int fpsOpenId, int companyId = 1)
        {
            var periods = new List<Domain.Entities.FinancialPeriodMaster>();
            for (byte p = 1; p <= 12; p++)
            {
                var pStart = start.AddMonths(p - 1);
                periods.Add(new Domain.Entities.FinancialPeriodMaster
                {
                    CompanyId = companyId,
                    PeriodNumber = p,
                    PeriodName = pStart.ToString("MMM-yyyy"),
                    StartDate = pStart,
                    EndDate   = pStart.AddMonths(1).AddDays(-1),
                    StatusId  = fpsOpenId,
                    IsAdjustmentPeriod = false,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            }
            var p12 = periods[^1];
            periods.Add(new Domain.Entities.FinancialPeriodMaster
            {
                CompanyId = companyId,
                PeriodNumber = 13,
                PeriodName = "Adj-2024-25",
                StartDate = p12.StartDate,
                EndDate   = p12.EndDate,
                StatusId  = fpsOpenId,
                IsAdjustmentPeriod = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            return periods;
        }

        // --- CREATE (atomic year + 13 periods) ---

        [Fact]
        public async Task CreateAsync_Should_Persist_Year_And_All_13_Periods_Atomically()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var year = BuildYear(ids.FysOpenId);
            var periods = Build13Periods(year.StartDate, ids.FpsOpenId);

            var newId = await CreateRepository(ctx).CreateAsync(year, periods, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            newId.Should().BeGreaterThan(0);

            var persistedPeriods = await ctx.FinancialPeriodMaster
                .Where(p => p.FinancialYearId == newId)
                .ToListAsync();

            persistedPeriods.Should().HaveCount(13);
            persistedPeriods.Count(p => p.IsAdjustmentPeriod).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_Should_Wire_All_Periods_To_NewYearId()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var year = BuildYear(ids.FysOpenId);
            var periods = Build13Periods(year.StartDate, ids.FpsOpenId);

            var newId = await CreateRepository(ctx).CreateAsync(year, periods, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var allFks = await ctx.FinancialPeriodMaster
                .Where(p => p.FinancialYearId == newId)
                .Select(p => p.FinancialYearId)
                .ToListAsync();

            allFks.Should().OnlyContain(fk => fk == newId);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields_From_DbContext()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var year = BuildYear(ids.FysOpenId);
            var periods = Build13Periods(year.StartDate, ids.FpsOpenId);

            var newId = await CreateRepository(ctx).CreateAsync(year, periods, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.FinancialYearMaster.FirstOrDefaultAsync(x => x.Id == newId);
            saved!.CreatedBy.Should().Be(1);                       // mocked GetUserId() in DbFixture
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE (FinancialYearCode + IsActive editable; StartDate/EndDate immutable) ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_FinancialYearCode_Change()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var year = BuildYear(ids.FysOpenId);
            var periods = Build13Periods(year.StartDate, ids.FpsOpenId);
            var id = await CreateRepository(ctx).CreateAsync(year, periods, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.FinancialYearMaster
            {
                Id = id,
                FinancialYearCode = "2024-99",
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.FinancialYearMaster.FirstAsync(x => x.Id == id);
            updated.FinancialYearCode.Should().Be("2024-99");
        }

        [Fact]
        public async Task UpdateAsync_Should_NotChange_StartDate_Or_EndDate()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var year = BuildYear(ids.FysOpenId);
            var periods = Build13Periods(year.StartDate, ids.FpsOpenId);
            var id = await CreateRepository(ctx).CreateAsync(year, periods, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            // Try to update dates — repo intentionally ignores them
            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.FinancialYearMaster
            {
                Id = id,
                FinancialYearCode = "2024-25",
                StartDate = new DateOnly(2099, 1, 1),       // ignored
                EndDate   = new DateOnly(2099, 12, 31),     // ignored
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.FinancialYearMaster.FirstAsync(x => x.Id == id);
            updated.StartDate.Should().Be(new DateOnly(2024, 4, 1));
            updated.EndDate.Should().Be(new DateOnly(2025, 3, 31));
        }

        // --- SOFT DELETE (cascades to all 13 periods) ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Cascade_IsDeleted_To_All_13_Periods()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var year = BuildYear(ids.FysOpenId);
            var periods = Build13Periods(year.StartDate, ids.FpsOpenId);
            var id = await CreateRepository(ctx).CreateAsync(year, periods, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var ok = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            ok.Should().BeTrue();
            var periodFlags = await ctx.FinancialPeriodMaster
                .Where(p => p.FinancialYearId == id)
                .Select(p => p.IsDeleted)
                .ToListAsync();

            periodFlags.Should().OnlyContain(d => d == IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_NonExistentId_ReturnsFalse()
        {
            await _fixture.ClearAllTablesAsync();
            await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var ok = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);
            ok.Should().BeFalse();
        }
    }
}
