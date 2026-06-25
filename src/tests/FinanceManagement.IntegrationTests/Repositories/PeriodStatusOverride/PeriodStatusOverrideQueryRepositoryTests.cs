using FinanceManagement.Infrastructure.Repositories.FinancialYearMaster;
using FinanceManagement.Infrastructure.Repositories.PeriodStatusOverride;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.FinancialYearMaster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.PeriodStatusOverride
{
    [Collection("DatabaseCollection")]
    public sealed class PeriodStatusOverrideQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public PeriodStatusOverrideQueryRepositoryTests(DbFixture fixture) { _fixture = fixture; }

        private PeriodStatusOverrideQueryRepository CreateRepository() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<(int periodId, (int FysOpenId, int FysClosedId, int FpsOpenId, int FpsSoftClosedId, int FpsHardClosedId,
            int PsoPendingId, int PsoFullyApprovedId, int PsoAppliedId, int PsoRejectedId) ids)>
            SeedAsync()
        {
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var year = new Domain.Entities.FinancialYearMaster
            {
                CompanyId = 1,
                FinancialYearCode = "2024-25",
                StartDate = new DateOnly(2024, 4, 1),
                EndDate = new DateOnly(2025, 3, 31),
                StatusId = ids.FysOpenId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var periods = new List<Domain.Entities.FinancialPeriodMaster>();
            var pStart = year.StartDate;
            periods.Add(new Domain.Entities.FinancialPeriodMaster
            {
                CompanyId = 1, PeriodNumber = 1, PeriodName = "Apr-2024",
                StartDate = pStart, EndDate = pStart.AddMonths(1).AddDays(-1),
                StatusId = ids.FpsHardClosedId,
                IsAdjustmentPeriod = false,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            });
            var yearId = await new FinancialYearMasterCommandRepository(ctx).CreateAsync(year, periods, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var pid = await ctx.FinancialPeriodMaster
                .Where(p => p.FinancialYearId == yearId)
                .Select(p => p.Id)
                .FirstAsync();

            return (pid, ids);
        }

        // --- GetPeriodSnapshotAsync ---

        [Fact]
        public async Task GetPeriodSnapshotAsync_HardClosedPeriod_ReturnsCorrectSnapshot()
        {
            await _fixture.ClearAllTablesAsync();
            var (periodId, _) = await SeedAsync();

            var snap = await CreateRepository().GetPeriodSnapshotAsync(periodId, CancellationToken.None);

            snap.Should().NotBeNull();
            snap!.Id.Should().Be(periodId);
            snap.CompanyId.Should().Be(1);
            snap.StatusCode.Should().Be("HARDCLOSED");
            snap.IsAdjustmentPeriod.Should().BeFalse();
        }

        [Fact]
        public async Task GetPeriodSnapshotAsync_NonExistent_ReturnsNull()
        {
            await _fixture.ClearAllTablesAsync();
            await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            var snap = await CreateRepository().GetPeriodSnapshotAsync(99999, CancellationToken.None);
            snap.Should().BeNull();
        }

        // --- HasOpenOverrideAsync ---

        [Fact]
        public async Task HasOpenOverrideAsync_PendingExists_ReturnsTrue()
        {
            await _fixture.ClearAllTablesAsync();
            var (periodId, ids) = await SeedAsync();

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await new PeriodStatusOverrideCommandRepository(ctx).CreateAsync(new Domain.Entities.PeriodStatusOverride
                {
                    FinancialPeriodId = periodId, CompanyId = 1,
                    FromStatusId = ids.FpsHardClosedId, ToStatusId = ids.FpsSoftClosedId,
                    RequestedBy = 1, RequestedAt = DateTimeOffset.UtcNow, RequestedReason = "X",
                    OverrideStatusId = ids.PsoPendingId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);
            }

            (await CreateRepository().HasOpenOverrideAsync(periodId)).Should().BeTrue();
        }

        [Fact]
        public async Task HasOpenOverrideAsync_RejectedOnly_ReturnsFalse()
        {
            await _fixture.ClearAllTablesAsync();
            var (periodId, ids) = await SeedAsync();

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await new PeriodStatusOverrideCommandRepository(ctx).CreateAsync(new Domain.Entities.PeriodStatusOverride
                {
                    FinancialPeriodId = periodId, CompanyId = 1,
                    FromStatusId = ids.FpsHardClosedId, ToStatusId = ids.FpsSoftClosedId,
                    RequestedBy = 1, RequestedAt = DateTimeOffset.UtcNow, RequestedReason = "X",
                    OverrideStatusId = ids.PsoRejectedId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);
            }

            (await CreateRepository().HasOpenOverrideAsync(periodId)).Should().BeFalse();
        }

        [Fact]
        public async Task HasOpenOverrideAsync_AppliedOnly_ReturnsFalse()
        {
            await _fixture.ClearAllTablesAsync();
            var (periodId, ids) = await SeedAsync();

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                await new PeriodStatusOverrideCommandRepository(ctx).CreateAsync(new Domain.Entities.PeriodStatusOverride
                {
                    FinancialPeriodId = periodId, CompanyId = 1,
                    FromStatusId = ids.FpsHardClosedId, ToStatusId = ids.FpsSoftClosedId,
                    RequestedBy = 1, RequestedAt = DateTimeOffset.UtcNow, RequestedReason = "X",
                    OverrideStatusId = ids.PsoAppliedId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);
            }

            (await CreateRepository().HasOpenOverrideAsync(periodId)).Should().BeFalse();
        }

        // --- GetMiscMasterIdByCodeAsync ---

        [Fact]
        public async Task GetMiscMasterIdByCodeAsync_PSO_Pending_ResolvesCorrectly()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            var id = await CreateRepository().GetMiscMasterIdByCodeAsync("PSO", "PENDING");
            id.Should().Be(ids.PsoPendingId);
        }

        // --- GetPendingForCompanyAsync ---

        [Fact]
        public async Task GetPendingForCompanyAsync_OnlyReturnsPending_NotAppliedOrRejected()
        {
            await _fixture.ClearAllTablesAsync();
            var (periodId, ids) = await SeedAsync();

            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var repo = new PeriodStatusOverrideCommandRepository(ctx);
                // pending
                await repo.CreateAsync(new Domain.Entities.PeriodStatusOverride
                {
                    FinancialPeriodId = periodId, CompanyId = 1,
                    FromStatusId = ids.FpsHardClosedId, ToStatusId = ids.FpsSoftClosedId,
                    RequestedBy = 1, RequestedAt = DateTimeOffset.UtcNow, RequestedReason = "A",
                    OverrideStatusId = ids.PsoPendingId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);
                // rejected
                await repo.CreateAsync(new Domain.Entities.PeriodStatusOverride
                {
                    FinancialPeriodId = periodId, CompanyId = 1,
                    FromStatusId = ids.FpsHardClosedId, ToStatusId = ids.FpsSoftClosedId,
                    RequestedBy = 1, RequestedAt = DateTimeOffset.UtcNow, RequestedReason = "B",
                    OverrideStatusId = ids.PsoRejectedId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);
            }

            var pending = await CreateRepository().GetPendingForCompanyAsync(companyId: 1, CancellationToken.None);
            pending.Should().HaveCount(1);
            pending[0].RequestedReason.Should().Be("A");
        }
    }
}
