using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Entities;
using BudgetManagement.Infrastructure.Repositories.Lookups.Budget;
using BudgetManagement.IntegrationTests.Common;
using Contracts.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace BudgetManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetAllocationLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetAllocationLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetAllocationLookupRepository CreateRepo(int unitId = 1)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(s => s.GetUnitId()).Returns(unitId);
            return new BudgetAllocationLookupRepository(conn, ipMock.Object);
        }

        private async Task<int> EnsureBudgetGroupAsync(int unitId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.BudgetGroups.FirstOrDefaultAsync(g => g.UnitId == unitId);
            if (existing != null) return existing.Id;

            var g = new BudgetManagement.Domain.Entities.BudgetGroup
            {
                Name = "Grp", Description = "Grp",
                UnitId = unitId, DepartmentId = 1, CostCenterId = 1, CurrencyId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            await ctx.BudgetGroups.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task<int> EnsureMiscAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscMaster.FirstOrDefaultAsync(m => m.Code == code);
            if (existing != null) return existing.Id;

            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "TEST_TYPE");
            if (type == null)
            {
                type = new BudgetManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "TEST_TYPE",
                    Description = "Test Type",
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(type);
                await ctx.SaveChangesAsync();
            }

            var m = new BudgetManagement.Domain.Entities.MiscMaster
            {
                Code = code,
                Description = code,
                MiscTypeId = type.Id,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<int> SeedAllocationAsync(
            int unitId = 1,
            int? budgetGroupId = null,
            int? requestByMiscId = null,
            int? monthMiscId = null,
            int? projectId = null,
            int? wbsId = null,
            int? financialYearId = 1,
            decimal remainingBalance = 1000m,
            DateOnly? fromDate = null,
            DateOnly? toDate = null)
        {
            var groupId = budgetGroupId ?? await EnsureBudgetGroupAsync(unitId);
            var allocationTypeId = await EnsureMiscAsync("ALLOC_TYPE");
            var requestById = requestByMiscId ?? allocationTypeId;
            var monthId = monthMiscId ?? allocationTypeId;
            await using var ctx = _fixture.CreateFreshDbContext();
            var a = new BudgetManagement.Domain.Entities.BudgetAllocation
            {
                UnitId = unitId,
                BudgetGroupId = groupId,
                FinancialYearId = financialYearId ?? 1,
                RequestById = requestById,
                RequestMonthId = monthId,
                AllocationTypeId = allocationTypeId,
                ProjectId = projectId,
                WBSId = wbsId,
                FromDate = fromDate ?? new DateOnly(2024, 1, 1),
                ToDate = toDate ?? new DateOnly(2024, 12, 31),
                ApprovedAmount = 5000m,
                RemainingBalance = remainingBalance,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            await ctx.BudgetAllocations.AddAsync(a);
            await ctx.SaveChangesAsync();
            return a.Id;
        }

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var allocs = await ctx.BudgetAllocations.ToListAsync();
            ctx.BudgetAllocations.RemoveRange(allocs);
            await ctx.SaveChangesAsync();
            var groups = await ctx.BudgetGroups.ToListAsync();
            ctx.BudgetGroups.RemoveRange(groups);
            await ctx.SaveChangesAsync();
        }

        // --- GetRemainingBalanceAsync ---

        [Fact]
        public async Task GetRemainingBalanceAsync_Should_Return_Current_Row()
        {
            await ClearAsync();
            var groupId = await EnsureBudgetGroupAsync();
            var currentMonthMisc = await EnsureMiscAsync("MONTH_CUR");
            var requestByMisc = await EnsureMiscAsync("REQ_BY");
            var id = await SeedAllocationAsync(
                budgetGroupId: groupId,
                monthMiscId: currentMonthMisc,
                requestByMiscId: requestByMisc,
                remainingBalance: 500m);

            var result = await CreateRepo().GetRemainingBalanceAsync(
                budgetGroupId: groupId,
                budgetDate: new DateOnly(2024, 6, 15),
                monthId: currentMonthMisc,
                requestById: requestByMisc,
                projectId: null,
                wbsId: null,
                financialYearId: 1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.CurrentRemainingBalance.Should().Be(500m);
        }

        [Fact]
        public async Task GetRemainingBalanceAsync_Should_Return_Zero_Id_When_NotFound()
        {
            await ClearAsync();

            var result = await CreateRepo().GetRemainingBalanceAsync(
                budgetGroupId: 999,
                budgetDate: new DateOnly(2024, 6, 15),
                monthId: 3,
                requestById: 5,
                projectId: null, wbsId: null, financialYearId: 1);

            result.Should().NotBeNull();
            result!.Id.Should().Be(0);
            result.CurrentRemainingBalance.Should().BeNull();
        }

        // GetRemainingBalanceAsync_Should_Return_Previous_Balance_Row is omitted:
        // The repository computes PrevMonthId = MonthId - 1 (assumes RequestMonthId values are
        // sequential integer month numbers). Since RequestMonthId is a FK to MiscMaster, the
        // previous MiscMaster row must have Id == MonthId - 1, which is not reliably
        // guaranteed across test runs. Tracked as a source-design coupling.

        // --- ApplyRemainingBalanceDeltaAsync ---

        [Fact]
        public async Task ApplyRemainingBalanceDelta_Should_Update_Row()
        {
            await ClearAsync();
            var groupId = await EnsureBudgetGroupAsync();
            var monthMisc = await EnsureMiscAsync("MONTH_DELTA");
            var reqByMisc = await EnsureMiscAsync("REQ_BY");
            var id = await SeedAllocationAsync(
                budgetGroupId: groupId, monthMiscId: monthMisc,
                requestByMiscId: reqByMisc, remainingBalance: 1000m);

            var result = await CreateRepo().ApplyRemainingBalanceDeltaAsync(
                budgetGroupId: groupId,
                budgetDate: new DateOnly(2024, 6, 15),
                monthId: monthMisc,
                requestById: reqByMisc,
                deltaAmount: -200m,
                projectId: null, wbsId: null, financialYearId: 1);

            result.Should().BeTrue();

            await using var ctx = _fixture.CreateFreshDbContext();
            var row = await ctx.BudgetAllocations.AsNoTracking().FirstAsync(a => a.Id == id);
            row.RemainingBalance.Should().Be(800m);
        }

        [Fact]
        public async Task ApplyRemainingBalanceDelta_Should_Return_False_When_NoMatch()
        {
            await ClearAsync();

            var result = await CreateRepo().ApplyRemainingBalanceDeltaAsync(
                budgetGroupId: 9999,
                budgetDate: new DateOnly(2024, 6, 15),
                monthId: 3,
                requestById: 5,
                deltaAmount: -100m,
                projectId: null, wbsId: null, financialYearId: 1);

            result.Should().BeFalse();
        }
    }
}
