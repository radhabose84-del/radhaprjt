using BudgetManagement.Infrastructure.Repositories.BudgetAllocation;
using BudgetManagement.IntegrationTests.Common;
using Contracts.Interfaces;
using Microsoft.Data.SqlClient;

namespace BudgetManagement.IntegrationTests.Repositories.BudgetAllocation
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetAllocationQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BudgetAllocationQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BudgetAllocationQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new BudgetAllocationQueryRepository(conn, _fixture.IpMock.Object, _fixture.DbContext);
        }

        private static BudgetManagement.Domain.Entities.BudgetAllocation BuildEntity(
            int unitId = 1,
            int financialYearId = 1,
            int requestById = 1,
            int requestMonthId = 1,
            int budgetGroupId = 1,
            int allocationTypeId = 1) =>
            new()
            {
                FinancialYearId = financialYearId,
                RequestById = requestById,
                RequestMonthId = requestMonthId,
                UnitId = unitId,
                BudgetGroupId = budgetGroupId,
                AllocationTypeId = allocationTypeId,
                ApprovedAmount = 50000m,
                RemainingBalance = 50000m,
                FromDate = DateOnly.FromDateTime(DateTime.Today),
                ToDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<int> SeedEntityAsync(
            int unitId = 1, int financialYearId = 1, int requestById = 1,
            int requestMonthId = 1, int budgetGroupId = 1, int allocationTypeId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new BudgetAllocationCommandRepository(ctx);
            return await repo.CreateAsync(BuildEntity(unitId, financialYearId, requestById,
                requestMonthId, budgetGroupId, allocationTypeId));
        }

        private async Task ClearTableAsync()
        {
            await _fixture.ClearTablesAsync("Budget.BudgetAllocation");
            await _fixture.SeedPrerequisiteDataAsync();
        }

        // --- EXISTS ---

        [Fact]
        public async Task ExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync(unitId: 1, financialYearId: 1, requestById: 1,
                requestMonthId: 1, budgetGroupId: 1, allocationTypeId: 1);

            var result = await CreateQueryRepo().ExistsAsync(
                unitId: 1,
                financialYearId: 1,
                requestById: 1,
                requestMonthId: 1,
                budgetGroupId: 1,
                allocationTypeId: 1,
                CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_Should_Return_False_When_No_Match()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().ExistsAsync(
                unitId: 99,
                financialYearId: 99,
                requestById: 99,
                requestMonthId: 99,
                budgetGroupId: 99,
                allocationTypeId: 99,
                CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}

