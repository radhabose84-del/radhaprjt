using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.FinancialYearMaster;
using FinanceManagement.Infrastructure.Repositories.PeriodStatusOverride;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.IntegrationTests.Repositories.FinancialYearMaster;
using Microsoft.EntityFrameworkCore;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.PeriodStatusOverride
{
    [Collection("DatabaseCollection")]
    public sealed class PeriodStatusOverrideCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public PeriodStatusOverrideCommandRepositoryTests(DbFixture fixture) { _fixture = fixture; }

        private async Task<(int yearId, int firstPeriodId, (int FysOpenId, int FysClosedId, int FpsOpenId, int FpsSoftClosedId, int FpsHardClosedId,
            int PsoPendingId, int PsoFullyApprovedId, int PsoAppliedId, int PsoRejectedId) ids)>
            SeedYearWithPeriodsAsync()
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
                    CompanyId = 1,
                    PeriodNumber = p,
                    PeriodName = pStart.ToString("MMM-yyyy"),
                    StartDate = pStart,
                    EndDate = pStart.AddMonths(1).AddDays(-1),
                    StatusId = ids.FpsOpenId,
                    IsAdjustmentPeriod = false,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            }
            var yearId = await new FinancialYearMasterCommandRepository(ctx).CreateAsync(year, periods, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var firstPeriodId = await ctx.FinancialPeriodMaster
                .Where(p => p.FinancialYearId == yearId)
                .OrderBy(p => p.PeriodNumber)
                .Select(p => p.Id)
                .FirstAsync();

            return (yearId, firstPeriodId, ids);
        }

        // --- ApplyPeriodStatusChangeAsync — the critical atomic transaction test ---

        [Fact]
        public async Task ApplyPeriodStatusChangeAsync_NoOverride_FlipsStatus_AndStampsLastChanged()
        {
            await _fixture.ClearAllTablesAsync();
            var (_, periodId, ids) = await SeedYearWithPeriodsAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new PeriodStatusOverrideCommandRepository(ctx);

            var changedAt = DateTimeOffset.UtcNow;
            var ok = await repo.ApplyPeriodStatusChangeAsync(
                financialPeriodId: periodId,
                newStatusId: ids.FpsSoftClosedId,
                changedBy: 42,
                changedAt: changedAt,
                overrideIdToMarkApplied: null,
                appliedStatusIdForOverride: null,
                ct: CancellationToken.None);

            ok.Should().BeTrue();
            ctx.ChangeTracker.Clear();

            var period = await ctx.FinancialPeriodMaster.FirstAsync(p => p.Id == periodId);
            period.StatusId.Should().Be(ids.FpsSoftClosedId);
            period.LastStatusChangedBy.Should().Be(42);
            period.LastStatusChangedAt.Should().BeCloseTo(changedAt, TimeSpan.FromSeconds(2));
        }

        [Fact]
        public async Task ApplyPeriodStatusChangeAsync_WithOverride_FlipsPeriod_AndMarksOverrideApplied_Atomically()
        {
            await _fixture.ClearAllTablesAsync();
            var (_, periodId, ids) = await SeedYearWithPeriodsAsync();

            // 1. Create a PENDING override
            await using (var ctxSeed = _fixture.CreateFreshDbContext())
            {
                var seedRepo = new PeriodStatusOverrideCommandRepository(ctxSeed);
                await seedRepo.CreateAsync(new Domain.Entities.PeriodStatusOverride
                {
                    FinancialPeriodId = periodId,
                    CompanyId = 1,
                    FromStatusId = ids.FpsHardClosedId,
                    ToStatusId = ids.FpsSoftClosedId,
                    RequestedBy = 1,
                    RequestedAt = DateTimeOffset.UtcNow,
                    RequestedReason = "Audit correction",
                    OverrideStatusId = ids.PsoPendingId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);
            }

            int overrideId;
            await using (var ctxLookup = _fixture.CreateFreshDbContext())
            {
                overrideId = await ctxLookup.PeriodStatusOverride
                    .Where(o => o.FinancialPeriodId == periodId)
                    .OrderByDescending(o => o.Id)
                    .Select(o => o.Id)
                    .FirstAsync();
            }

            // 2. Apply — period status flip + override marked APPLIED in one transaction
            await using (var ctxApply = _fixture.CreateFreshDbContext())
            {
                var ok = await new PeriodStatusOverrideCommandRepository(ctxApply).ApplyPeriodStatusChangeAsync(
                    financialPeriodId: periodId,
                    newStatusId: ids.FpsSoftClosedId,
                    changedBy: 99,
                    changedAt: DateTimeOffset.UtcNow,
                    overrideIdToMarkApplied: overrideId,
                    appliedStatusIdForOverride: ids.PsoAppliedId,
                    ct: CancellationToken.None);
                ok.Should().BeTrue();
            }

            // 3. Verify both writes succeeded together
            await using var ctxVerify = _fixture.CreateFreshDbContext();
            var period = await ctxVerify.FinancialPeriodMaster.FirstAsync(p => p.Id == periodId);
            period.StatusId.Should().Be(ids.FpsSoftClosedId);

            var ovr = await ctxVerify.PeriodStatusOverride.FirstAsync(o => o.Id == overrideId);
            ovr.OverrideStatusId.Should().Be(ids.PsoAppliedId);
            ovr.AppliedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task ApplyPeriodStatusChangeAsync_NonExistentPeriod_ReturnsFalseWithNoChanges()
        {
            await _fixture.ClearAllTablesAsync();
            var ids = await IntegrationTestSeeder.SeedStatusRowsAsync(_fixture);

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new PeriodStatusOverrideCommandRepository(ctx);

            var ok = await repo.ApplyPeriodStatusChangeAsync(
                financialPeriodId: 999999,
                newStatusId: ids.FpsSoftClosedId,
                changedBy: 1, changedAt: DateTimeOffset.UtcNow,
                overrideIdToMarkApplied: null, appliedStatusIdForOverride: null,
                ct: CancellationToken.None);

            ok.Should().BeFalse();
        }

        // --- CreateAsync + UpdateAsync ---

        [Fact]
        public async Task CreateAsync_Should_Insert_Override_With_Pending_Status()
        {
            await _fixture.ClearAllTablesAsync();
            var (_, periodId, ids) = await SeedYearWithPeriodsAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new PeriodStatusOverrideCommandRepository(ctx);

            var id = await repo.CreateAsync(new Domain.Entities.PeriodStatusOverride
            {
                FinancialPeriodId = periodId,
                CompanyId = 1,
                FromStatusId = ids.FpsHardClosedId,
                ToStatusId = ids.FpsSoftClosedId,
                RequestedBy = 42,
                RequestedAt = DateTimeOffset.UtcNow,
                RequestedReason = "Why",
                OverrideStatusId = ids.PsoPendingId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            }, CancellationToken.None);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Approver_Stamps()
        {
            await _fixture.ClearAllTablesAsync();
            var (_, periodId, ids) = await SeedYearWithPeriodsAsync();

            int id;
            await using (var ctxSeed = _fixture.CreateFreshDbContext())
            {
                id = await new PeriodStatusOverrideCommandRepository(ctxSeed).CreateAsync(new Domain.Entities.PeriodStatusOverride
                {
                    FinancialPeriodId = periodId,
                    CompanyId = 1,
                    FromStatusId = ids.FpsHardClosedId,
                    ToStatusId = ids.FpsSoftClosedId,
                    RequestedBy = 1,
                    RequestedAt = DateTimeOffset.UtcNow,
                    RequestedReason = "Why",
                    OverrideStatusId = ids.PsoPendingId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                }, CancellationToken.None);
            }

            var now = DateTimeOffset.UtcNow;
            await using (var ctxUpd = _fixture.CreateFreshDbContext())
            {
                await new PeriodStatusOverrideCommandRepository(ctxUpd).UpdateAsync(new Domain.Entities.PeriodStatusOverride
                {
                    Id = id,
                    FinancialPeriodId = periodId,
                    CompanyId = 1,
                    FromStatusId = ids.FpsHardClosedId,
                    ToStatusId = ids.FpsSoftClosedId,
                    RequestedBy = 1,
                    RequestedAt = now.AddMinutes(-5),
                    RequestedReason = "Why",
                    CfoApproverId = 50,
                    CfoApprovedAt = now,
                    OverrideStatusId = ids.PsoPendingId
                }, CancellationToken.None);
            }

            await using var ctxVerify = _fixture.CreateFreshDbContext();
            var ovr = await ctxVerify.PeriodStatusOverride.FirstAsync(o => o.Id == id);
            ovr.CfoApproverId.Should().Be(50);
            ovr.CfoApprovedAt.Should().NotBeNull();
        }
    }
}
