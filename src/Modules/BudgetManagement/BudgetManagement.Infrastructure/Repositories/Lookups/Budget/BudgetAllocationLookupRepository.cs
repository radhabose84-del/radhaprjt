using System.Data;
using System.Data.Common;
using Contracts.Interfaces;
using Contracts.Dtos.Budget;
using Contracts.Interfaces.Lookups.Budget;
using Dapper;

namespace BudgetManagement.Infrastructure.Repositories.Lookups.Budget
{
    internal class BudgetAllocationLookupRepository : IBudgetAllocationLookup
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;

        public BudgetAllocationLookupRepository(
            IDbConnection dbConnection,
            IIPAddressService ipAddressService)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
        }

        public async Task<RemainingBalanceWithPrevDto?> GetRemainingBalanceAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int monthId,
            int requestById,
            int? projectId,
            int? wbsId,
            int? financialYearId,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // Treat 0 as null
            projectId = (projectId.HasValue && projectId.Value <= 0) ? null : projectId;
            wbsId = (wbsId.HasValue && wbsId.Value <= 0) ? null : wbsId;
            int? monthIdNullable = monthId <= 0 ? null : monthId;
            int? requestByIdNullable = requestById <= 0 ? null : requestById;

            int? prevMonthId = monthIdNullable.HasValue ? monthIdNullable.Value - 1 : null;

            DateTime sqlDate = budgetDate.ToDateTime(TimeOnly.MinValue);

            const string sql = @"
                -- Current
                SELECT TOP 1 Id, RemainingBalance
                FROM Budget.BudgetAllocation
                WHERE UnitId = @UnitId
                AND BudgetGroupId = @BudgetGroupId
                AND (@RequestById IS NULL OR RequestById = @RequestById)
                AND @BudgetDate BETWEEN FromDate AND ToDate
                AND (@MonthId IS NULL OR RequestMonthId = @MonthId)
                AND (@ProjectId IS NULL OR ProjectId = @ProjectId)
                AND (@WbsId IS NULL OR WBSId = @WbsId)
                AND (@FinancialYearId IS NULL OR FinancialYearId = @FinancialYearId)
                ORDER BY Id DESC;

                -- Previous
                SELECT TOP 1 Id, RemainingBalance
                FROM Budget.BudgetAllocation
                WHERE UnitId = @UnitId
                AND BudgetGroupId = @BudgetGroupId
                AND (@RequestById IS NULL OR RequestById = @RequestById)
                AND @BudgetDate BETWEEN FromDate AND ToDate
                AND (@PrevMonthId IS NULL OR RequestMonthId = @PrevMonthId)
                AND (@ProjectId IS NULL OR ProjectId = @ProjectId)
                AND (@WbsId IS NULL OR WBSId = @WbsId)
                AND (@FinancialYearId IS NULL OR FinancialYearId = @FinancialYearId)
                ORDER BY Id DESC;
            ";

            using var multi = await _dbConnection.QueryMultipleAsync(
                new CommandDefinition(sql, new
                {
                    UnitId = unitId,
                    BudgetGroupId = budgetGroupId,
                    MonthId = monthIdNullable,
                    PrevMonthId = prevMonthId,
                    RequestById = requestByIdNullable,
                    BudgetDate = sqlDate,
                    ProjectId = projectId,
                    WbsId = wbsId,
                    FinancialYearId = financialYearId
                }, cancellationToken: ct));

            var currentRow = await multi.ReadFirstOrDefaultAsync<(int Id, decimal RemainingBalance)?>();
            var prevRow = await multi.ReadFirstOrDefaultAsync<(int Id, decimal RemainingBalance)?>();

            return new RemainingBalanceWithPrevDto
            {
                BudgetGroupId = budgetGroupId,
                BudgetDate = budgetDate,
                MonthId = monthIdNullable,
                RequestById = requestByIdNullable,
                ProjectId = projectId,
                WbsId = wbsId,
                FinancialYearId = financialYearId,

                Id = currentRow?.Id ?? 0,
                CurrentRemainingBalance = currentRow?.RemainingBalance,

                PreviousId = prevRow?.Id,
                PreviousRemainingBalance = prevRow?.RemainingBalance
            };
        }

        public async Task<BudgetAllocationSummaryDto?> GetAllocationSummaryAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int monthId,
            int requestById,
            int? projectId,
            int? wbsId,
            int? financialYearId,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            projectId = (projectId.HasValue && projectId.Value <= 0) ? null : projectId;
            wbsId = (wbsId.HasValue && wbsId.Value <= 0) ? null : wbsId;
            int? monthIdNullable = monthId <= 0 ? null : monthId;
            int? requestByIdNullable = requestById <= 0 ? null : requestById;

            DateTime sqlDate = budgetDate.ToDateTime(TimeOnly.MinValue);

            const string sql = @"
                SELECT TOP 1 ApprovedAmount, ISNULL(RemainingBalance, 0) AS RemainingBalance
                FROM Budget.BudgetAllocation
                WHERE UnitId = @UnitId
                AND BudgetGroupId = @BudgetGroupId
                AND (@RequestById IS NULL OR RequestById = @RequestById)
                AND @BudgetDate BETWEEN FromDate AND ToDate
                AND (@MonthId IS NULL OR RequestMonthId = @MonthId)
                AND (@ProjectId IS NULL OR ProjectId = @ProjectId)
                AND (@WbsId IS NULL OR WBSId = @WbsId)
                AND (@FinancialYearId IS NULL OR FinancialYearId = @FinancialYearId)
                ORDER BY Id DESC;";

            return await _dbConnection.QueryFirstOrDefaultAsync<BudgetAllocationSummaryDto>(
                new CommandDefinition(sql, new
                {
                    UnitId = unitId,
                    BudgetGroupId = budgetGroupId,
                    MonthId = monthIdNullable,
                    RequestById = requestByIdNullable,
                    BudgetDate = sqlDate,
                    ProjectId = projectId,
                    WbsId = wbsId,
                    FinancialYearId = financialYearId
                }, cancellationToken: ct));
        }

        public async Task<bool> ApplyRemainingBalanceDeltaAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int monthId,
            int requestById,
            decimal deltaAmount,
            int? projectId,
            int? wbsId,
            int? financialYearId,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // Treat 0 as null
            projectId = (projectId.HasValue && projectId.Value <= 0) ? null : projectId;
            wbsId = (wbsId.HasValue && wbsId.Value <= 0) ? null : wbsId;
            int? monthIdNullable = monthId <= 0 ? null : monthId;
            int? requestByIdNullable = requestById <= 0 ? null : requestById;

            DateTime sqlDate = budgetDate.ToDateTime(TimeOnly.MinValue);

            const string sql = @"
                UPDATE Budget.BudgetAllocation
                SET RemainingBalance = ISNULL(RemainingBalance, 0) + @DeltaAmount
                WHERE UnitId = @UnitId
                AND BudgetGroupId = @BudgetGroupId
                AND (@RequestById IS NULL OR RequestById = @RequestById)
                AND @BudgetDate BETWEEN FromDate AND ToDate
                AND (@MonthId IS NULL OR RequestMonthId = @MonthId)
                AND (@ProjectId IS NULL OR ProjectId = @ProjectId)
                AND (@WbsId IS NULL OR WBSId = @WbsId)
                AND (@FinancialYearId IS NULL OR FinancialYearId = @FinancialYearId);
            ";

            var rowsAffected = await _dbConnection.ExecuteAsync(
                new CommandDefinition(sql, new
                {
                    UnitId = unitId,
                    BudgetGroupId = budgetGroupId,
                    MonthId = monthIdNullable,
                    RequestById = requestByIdNullable,
                    BudgetDate = sqlDate,
                    DeltaAmount = deltaAmount,
                    ProjectId = projectId,
                    WbsId = wbsId,
                    FinancialYearId = financialYearId
                }, cancellationToken: ct));

            return rowsAffected > 0;
        }

        public async Task<bool> ApplyRemainingBalanceDeltaAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int monthId,
            int requestById,
            decimal deltaAmount,
            int? projectId,
            int? wbsId,
            int? financialYearId,
            DbConnection connection,
            DbTransaction transaction,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            projectId = (projectId.HasValue && projectId.Value <= 0) ? null : projectId;
            wbsId = (wbsId.HasValue && wbsId.Value <= 0) ? null : wbsId;
            int? monthIdNullable = monthId <= 0 ? null : monthId;
            int? requestByIdNullable = requestById <= 0 ? null : requestById;

            DateTime sqlDate = budgetDate.ToDateTime(TimeOnly.MinValue);

            const string sql = @"
                UPDATE Budget.BudgetAllocation
                SET RemainingBalance = ISNULL(RemainingBalance, 0) + @DeltaAmount
                WHERE UnitId = @UnitId
                AND BudgetGroupId = @BudgetGroupId
                AND (@RequestById IS NULL OR RequestById = @RequestById)
                AND @BudgetDate BETWEEN FromDate AND ToDate
                AND (@MonthId IS NULL OR RequestMonthId = @MonthId)
                AND (@ProjectId IS NULL OR ProjectId = @ProjectId)
                AND (@WbsId IS NULL OR WBSId = @WbsId)
                AND (@FinancialYearId IS NULL OR FinancialYearId = @FinancialYearId);
            ";

            // Execute on the CALLER's connection + transaction (Shared Transaction pattern)
            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(sql, new
                {
                    UnitId = unitId,
                    BudgetGroupId = budgetGroupId,
                    MonthId = monthIdNullable,
                    RequestById = requestByIdNullable,
                    BudgetDate = sqlDate,
                    DeltaAmount = deltaAmount,
                    ProjectId = projectId,
                    WbsId = wbsId,
                    FinancialYearId = financialYearId
                }, transaction: transaction, cancellationToken: ct));

            return rowsAffected > 0;
        }
    }
}
