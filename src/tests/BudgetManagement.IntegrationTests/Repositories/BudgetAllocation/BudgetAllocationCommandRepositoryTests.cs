using BudgetManagement.Infrastructure.Repositories.BudgetAllocation;
using BudgetManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.IntegrationTests.Repositories.BudgetAllocation
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetAllocationCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetAllocationCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetAllocationCommandRepository CreateRepository(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static BudgetManagement.Domain.Entities.BudgetAllocation BuildEntity(
            int unitId = 1,
            int financialYearId = 1,
            int requestById = 1,
            decimal approvedAmount = 50000m) =>
            new()
            {
                FinancialYearId = financialYearId,
                RequestById = requestById,
                RequestMonthId = 1,
                UnitId = unitId,
                BudgetGroupId = 1,
                AllocationTypeId = 1,
                ApprovedAmount = approvedAmount,
                RemainingBalance = approvedAmount,
                FromDate = DateOnly.FromDateTime(DateTime.Today),
                ToDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Budget.BudgetAllocation");

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_RemainingBalance_EqualTo_ApprovedAmount()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(approvedAmount: 75000m));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BudgetAllocations.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.RemainingBalance.Should().Be(75000m);
            saved.ApprovedAmount.Should().Be(75000m);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(unitId: 1, requestById: 2));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BudgetAllocations.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.UnitId.Should().Be(1);
            saved.RequestById.Should().Be(2);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        // --- UPDATE REMAINING BALANCE ---

        [Fact]
        public async Task UpdateRemainingBalanceAsync_Should_Return_True()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(approvedAmount: 50000m));

            var result = await CreateRepository(ctx).UpdateRemainingBalanceAsync(newId, 30000m, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateRemainingBalanceAsync_Should_Persist_NewBalance()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(approvedAmount: 50000m));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateRemainingBalanceAsync(newId, 20000m, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BudgetAllocations.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.RemainingBalance.Should().Be(20000m);
        }

        // --- GET BY KEY ---

        [Fact]
        public async Task GetByKeyAsync_Should_Return_Entity_When_Match_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var entity = BuildEntity(unitId: 1, financialYearId: 2, requestById: 3);
            await CreateRepository(ctx).CreateAsync(entity);

            var result = await CreateRepository(ctx).GetByKeyAsync(
                unitId: 1,
                financialYearId: 2,
                requestMonthId: 1,
                budgetGroupId: 1,
                requestById: 3,
                fromDate: DateOnly.FromDateTime(DateTime.Today),
                toDate: DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                wbsId: null,
                projectId: null,
                ct: CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task GetByKeyAsync_Should_Return_Null_When_No_Match()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).GetByKeyAsync(
                unitId: 99,
                financialYearId: 99,
                requestMonthId: 99,
                budgetGroupId: 99,
                requestById: 99,
                fromDate: DateOnly.FromDateTime(DateTime.Today),
                toDate: DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                wbsId: null,
                projectId: null,
                ct: CancellationToken.None);

            result.Should().BeNull();
        }
    }
}

