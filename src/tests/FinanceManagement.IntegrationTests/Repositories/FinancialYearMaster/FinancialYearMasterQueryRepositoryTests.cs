using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Infrastructure.Repositories.FinancialYearMaster;
using FinanceManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Moq;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.FinancialYearMaster
{
    [Collection("DatabaseCollection")]
    public sealed class FinancialYearMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public FinancialYearMasterQueryRepositoryTests(DbFixture fixture) { _fixture = fixture; }

        private FinancialYearMasterQueryRepository CreateRepository()
        {
            var companyLookup = new Mock<ICompanyLookup>(MockBehavior.Loose);
            companyLookup.Setup(x => x.GetAllCompanyAsync()).ReturnsAsync(new List<CompanyLookupDto>
            {
                new() { CompanyId = 1, CompanyName = "Test Company" }
            });
            return new FinancialYearMasterQueryRepository(
                new SqlConnection(_fixture.ConnectionString), companyLookup.Object);
        }

        private async Task<int> SeedFinancialYearAsync(
            string code, DateOnly start, DateOnly end, int fysOpenId, int fpsOpenId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var year = new Domain.Entities.FinancialYearMaster
            {
                CompanyId = 1,
                FinancialYearCode = code,
                StartDate = start,
                EndDate = end,
                StatusId = fysOpenId,
                IsTransitionYear = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var periods = new List<Domain.Entities.FinancialPeriodMaster>();
            for (byte p = 1; p <= 12; p++)
            {
                var pStart = start.AddMonths(p - 1);
                periods.Add(new Domain.Entities.FinancialPeriodMaster
                {
                    CompanyId = 1,
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
                CompanyId = 1,
                PeriodNumber = 13,
                PeriodName = $"Adj-{code}",
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
        public async Task GetByIdAsync_Should_Return_Year_With_13_Periods()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            var id = await SeedFinancialYearAsync(
                "2024-25", new DateOnly(2024, 4, 1), new DateOnly(2025, 3, 31),
                ids.FysOpenId, ids.FpsOpenId);

            var dto = await CreateRepository().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Periods.Should().HaveCount(13);
            dto.Periods.Count(p => p.IsAdjustmentPeriod).Should().Be(1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_StatusCode_From_MiscMaster_Join()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            var id = await SeedFinancialYearAsync(
                "2024-25", new DateOnly(2024, 4, 1), new DateOnly(2025, 3, 31),
                ids.FysOpenId, ids.FpsOpenId);

            var dto = await CreateRepository().GetByIdAsync(id);
            dto!.StatusCode.Should().Be("OPEN");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_CompanyName_From_Lookup()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            var id = await SeedFinancialYearAsync(
                "2024-25", new DateOnly(2024, 4, 1), new DateOnly(2025, 3, 31),
                ids.FysOpenId, ids.FpsOpenId);

            var dto = await CreateRepository().GetByIdAsync(id);
            dto!.CompanyName.Should().Be("Test Company");
        }

        // --- GetPeriodForDateAsync (used by posting engine) ---

        [Fact]
        public async Task GetPeriodForDateAsync_DateInPeriod3_ReturnsJunePeriod()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            await SeedFinancialYearAsync(
                "2024-25", new DateOnly(2024, 4, 1), new DateOnly(2025, 3, 31),
                ids.FysOpenId, ids.FpsOpenId);

            var result = await CreateRepository()
                .GetPeriodForDateAsync(companyId: 1, date: new DateOnly(2024, 6, 15), ct: CancellationToken.None);

            result.Should().NotBeNull();
            result!.PeriodNumber.Should().Be(3);
            result.IsAdjustmentPeriod.Should().BeFalse();
        }

        [Fact]
        public async Task GetPeriodForDateAsync_Should_SkipAdjustmentPeriod()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            await SeedFinancialYearAsync(
                "2024-25", new DateOnly(2024, 4, 1), new DateOnly(2025, 3, 31),
                ids.FysOpenId, ids.FpsOpenId);

            // March 15 falls in Period 12 AND Period 13 (same span) — must return regular Period 12
            var result = await CreateRepository()
                .GetPeriodForDateAsync(companyId: 1, date: new DateOnly(2025, 3, 15), ct: CancellationToken.None);

            result.Should().NotBeNull();
            result!.PeriodNumber.Should().Be(12);
            result.IsAdjustmentPeriod.Should().BeFalse();
        }

        [Fact]
        public async Task GetPeriodForDateAsync_DateOutsideAnyPeriod_ReturnsNull()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            await SeedFinancialYearAsync(
                "2024-25", new DateOnly(2024, 4, 1), new DateOnly(2025, 3, 31),
                ids.FysOpenId, ids.FpsOpenId);

            var result = await CreateRepository()
                .GetPeriodForDateAsync(companyId: 1, date: new DateOnly(2030, 6, 15), ct: CancellationToken.None);

            result.Should().BeNull();
        }

        // --- OverlapsExistingRangeAsync ---

        [Fact]
        public async Task OverlapsExistingRangeAsync_FullyContainedNewYear_DetectsOverlap()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            await SeedFinancialYearAsync(
                "2024-25", new DateOnly(2024, 4, 1), new DateOnly(2025, 3, 31),
                ids.FysOpenId, ids.FpsOpenId);

            var overlaps = await CreateRepository().OverlapsExistingRangeAsync(
                new DateOnly(2024, 7, 1), new DateOnly(2024, 9, 30), companyId: 1);

            overlaps.Should().BeTrue();
        }

        [Fact]
        public async Task OverlapsExistingRangeAsync_AdjacentNonOverlapping_ReturnsFalse()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);
            await SeedFinancialYearAsync(
                "2024-25", new DateOnly(2024, 4, 1), new DateOnly(2025, 3, 31),
                ids.FysOpenId, ids.FpsOpenId);

            // Apr 1 2025 → Mar 31 2026 — picks up the day after the existing year ends
            var overlaps = await CreateRepository().OverlapsExistingRangeAsync(
                new DateOnly(2025, 4, 1), new DateOnly(2026, 3, 31), companyId: 1);

            overlaps.Should().BeFalse();
        }

        // --- Lookup helpers ---

        [Fact]
        public async Task FinancialYearStatusExistsAsync_TrueForFysCode_FalseForUnrelated()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            var repo = CreateRepository();
            (await repo.FinancialYearStatusExistsAsync(ids.FysOpenId)).Should().BeTrue();
            (await repo.FinancialYearStatusExistsAsync(ids.FpsOpenId)).Should().BeFalse();      // FPS-typed id, wrong type
        }

        [Fact]
        public async Task GetMiscMasterIdByCodeAsync_ResolvesCorrectId()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            var fysOpen = await CreateRepository().GetMiscMasterIdByCodeAsync("FYS", "OPEN");
            fysOpen.Should().Be(ids.FysOpenId);
        }

        [Fact]
        public async Task GetMiscMasterIdByCodeAsync_NotSeeded_ReturnsZero()
        {
            await _fixture.ClearAllTablesAsync();
            await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            var result = await CreateRepository().GetMiscMasterIdByCodeAsync("FYS", "DOESNOTEXIST");
            result.Should().Be(0);
        }
    }
}
