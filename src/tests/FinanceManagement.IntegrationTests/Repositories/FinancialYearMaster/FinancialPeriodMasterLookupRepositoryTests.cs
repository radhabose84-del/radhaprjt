using FinanceManagement.Infrastructure.Repositories.FinancialYearMaster;
using FinanceManagement.Infrastructure.Repositories.Lookups.Finance;
using FinanceManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.FinancialYearMaster
{
    [Collection("DatabaseCollection")]
    public sealed class FinancialPeriodMasterLookupRepositoryTests
    {
        private readonly DbFixture _fixture;
        public FinancialPeriodMasterLookupRepositoryTests(DbFixture fixture) { _fixture = fixture; }

        private FinancialPeriodMasterLookupRepository CreateRepository() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedYearAsync(int fysOpenId, int fpsOpenId, int companyId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var year = new Domain.Entities.FinancialYearMaster
            {
                CompanyId = companyId,
                FinancialYearCode = "2024-25",
                StartDate = new DateOnly(2024, 4, 1),
                EndDate = new DateOnly(2025, 3, 31),
                StatusId = fysOpenId,
                IsTransitionYear = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var periods = new List<Domain.Entities.FinancialPeriodMaster>();
            for (byte p = 1; p <= 12; p++)
            {
                var pStart = year.StartDate.AddMonths(p - 1);
                periods.Add(new Domain.Entities.FinancialPeriodMaster
                {
                    CompanyId = companyId,
                    PeriodNumber = p,
                    PeriodName = pStart.ToString("MMM-yyyy"),
                    StartDate = pStart,
                    EndDate = pStart.AddMonths(1).AddDays(-1),
                    StatusId = fpsOpenId,
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
                EndDate = p12.EndDate,
                StatusId = fpsOpenId,
                IsAdjustmentPeriod = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            var repo = new FinancialYearMasterCommandRepository(ctx);
            return await repo.CreateAsync(year, periods, CancellationToken.None);
        }

        [Fact]
        public async Task GetAllPeriodsForCompanyAsync_Should_Return_All_13_Periods()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            await SeedYearAsync(ids.FysOpenId, ids.FpsOpenId);

            var result = await CreateRepository()
                .GetAllPeriodsForCompanyAsync(companyId: 1, CancellationToken.None);

            result.Should().HaveCount(13);
            result.Count(p => p.IsAdjustmentPeriod).Should().Be(1);
        }

        [Fact]
        public async Task GetPeriodForDateAsync_Should_Return_RegularPeriod_NotAdjustment()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            await SeedYearAsync(ids.FysOpenId, ids.FpsOpenId);

            // March falls in both Period 12 and Period 13 — lookup must return regular Period 12
            var result = await CreateRepository().GetPeriodForDateAsync(
                companyId: 1, date: new DateOnly(2025, 3, 15), CancellationToken.None);

            result.Should().NotBeNull();
            result!.PeriodNumber.Should().Be(12);
            result.IsAdjustmentPeriod.Should().BeFalse();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Scope_To_Company()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            var yearId = await SeedYearAsync(ids.FysOpenId, ids.FpsOpenId, companyId: 1);

            // get any period id from that year for company 1
            await using var ctx = _fixture.CreateFreshDbContext();
            var anyPeriodId = ctx.FinancialPeriodMaster
                .First(p => p.FinancialYearId == yearId).Id;

            var hit = await CreateRepository().GetByIdAsync(anyPeriodId, companyId: 1, CancellationToken.None);
            hit.Should().NotBeNull();

            var miss = await CreateRepository().GetByIdAsync(anyPeriodId, companyId: 99, CancellationToken.None);
            miss.Should().BeNull();
        }
    }
}
