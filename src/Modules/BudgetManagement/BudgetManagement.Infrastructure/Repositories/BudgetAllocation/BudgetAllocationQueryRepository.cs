#nullable disable
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Application.BudgetAllocation.Queries.GetBudgetBalanceReport;
using BudgetManagement.Application.BudgetAllocation.Queries.GetRemainingBalance;
using BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleDetailsMonthwise;
using BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleMonthwiseReport;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using BudgetManagement.Domain.Common;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.Infrastructure.Repositories.BudgetAllocation
{
    public class BudgetAllocationQueryRepository : IBudgetAllocationQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IIPAddressService _ipAddressService;
        private readonly ApplicationDbContext _applicationDbContext;
        public BudgetAllocationQueryRepository(IDbConnection dbConnection, IIPAddressService ipAddressService, ApplicationDbContext applicationDbContext)
        {
            _dbConnection = dbConnection;
            _ipAddressService = ipAddressService;
            _applicationDbContext = applicationDbContext;
        }

        public async Task<bool> ExistsAsync(
         int unitId,
         int financialYearId,
         int requestById,
         int requestMonthId,
         int budgetGroupId,
         int allocationTypeId,
         CancellationToken ct = default)
        {
            var sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM Budget.BudgetAllocation
                WHERE UnitId = @UnitId
                AND FinancialYearId = @FinancialYearId
                AND RequestById = @RequestById
                AND RequestMonthId = @RequestMonthId
                AND BudgetGroupId = @BudgetGroupId
                AND AllocationTypeId = @AllocationTypeId
            ) THEN 1 ELSE 0 END";

            var result = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                UnitId = unitId,
                FinancialYearId = financialYearId,
                RequestById = requestById,
                RequestMonthId = requestMonthId,
                BudgetGroupId = budgetGroupId,
                AllocationTypeId = allocationTypeId
            });

            return result == 1;
        }


        public async Task<(List<GetSpindleDetailsMonthwiseDto>, int)> GetBudgetGroupDetailSpindlewise(
        int PageNumber, int PageSize, string SearchTerm)
        {
            var UnitId = _ipAddressService.GetUnitId();

            // Build Search Parameter
            string search = string.IsNullOrWhiteSpace(SearchTerm) ? null : $"%{SearchTerm}%";

            // Pagination
            int offset = (PageNumber - 1) * PageSize;

            const string dataQuery = @"
            SELECT 
            BG.Id AS BudgetGroupId,
            BG.Name AS BudgetGroupName,
            BG.ParentBudgetGroupId as BudgetSubGroupId,
            BG.UnitId,
            BG.DepartmentId,
            BG.CostCenterId,
            BG.CurrencyId,
            BG.AllocationRuleId as AllocationTypeId,
            MM.Description AS AllocationTypeName,
            BG.AllocatedSpindleCost AS RatePerSpindle
        FROM Budget.BudgetGroup BG
        INNER JOIN Budget.MiscMaster MM  
            ON BG.AllocationRuleId = MM.Id 
            AND MM.Description = @AllocationRule
        WHERE 
            BG.UnitId = @UnitId
            AND BG.IsDeleted = 0
            AND BG.IsActive = 1
            AND (@Search IS NULL 
                    OR BG.Name LIKE @Search
                    OR MM.Description LIKE @Search)
        ORDER BY BG.Id DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
    ";

            const string countQuery = @"
        SELECT COUNT(1)
        FROM Budget.BudgetGroup BG
        INNER JOIN Budget.MiscMaster MM  
            ON BG.AllocationRuleId = MM.Id 
            AND MM.Description = @AllocationRule
        WHERE 
            BG.UnitId = @UnitId
            AND BG.IsDeleted = 0
            AND BG.IsActive = 1
            AND (@Search IS NULL 
                    OR BG.Name LIKE @Search 
                    OR MM.Description LIKE @Search);
    ";

            var parameters = new
            {
                UnitId,
                Search = search,
                Offset = offset,
                PageSize,
                AllocationRule = MiscEnumEntity.AllocationTypeSpindle   // <-- REQUIRED
            };

            // Fetch paginated list
            var result = await _dbConnection.QueryAsync<GetSpindleDetailsMonthwiseDto>(dataQuery, parameters);

            // Fetch total count
            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countQuery, parameters);

            return (result.ToList(), totalCount);
        }

        public async Task<List<GetSpindleMonthwiseReportDto>> GetSpindleDetailsMonthwiseAsync(
                int financialYearId, int? departmentId, int? costCenterId, int? allocationTypeId, int? budgetGroupId, DateOnly? budgetDate,CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();
            var sql = @"
                    SELECT 
                        A.UnitId,
                        A.FinancialYearId,
                        A.BudgetGroupId,
                        BG.Name AS BudgetGroup,

                        A.RequestById,
                        RB.Description AS RequestBy,

                        A.RequestMonthId,
                        RM.Code AS MonthCode,
                        RM.Description AS RequestMonth,

                        A.AllocationTypeId,
                        AT.Description AS AllocationType,

                        A.SpindleCount,
                        A.RatePerSpindle,
                        A.FromDate,
                        A.ToDate,
                        A.ApprovedAmount,
                        BG.DepartmentId,
                        BG.CostCenterId,
                        BG.CurrencyId,A.RemainingBalance,A.RequestId
                    FROM Budget.BudgetAllocation A
                    INNER JOIN Budget.BudgetGroup BG 
                        ON A.BudgetGroupId = BG.Id
                    INNER JOIN Budget.MiscMaster RB 
                        ON RB.Id = A.RequestById
                    LEFT JOIN Budget.MiscMaster RM 
                        ON RM.Id = A.RequestMonthId
                    INNER JOIN Budget.MiscMaster AT 
                        ON AT.Id = A.AllocationTypeId
                    WHERE 
                        A.FinancialYearId = @FinancialYearId
                        AND (@DepartmentId IS NULL OR BG.DepartmentId = @DepartmentId)
                        AND (@CostCenterId IS NULL OR BG.CostCenterId = @CostCenterId)
                        AND (@AllocationTypeId IS NULL OR A.AllocationTypeId = @AllocationTypeId)
                        AND (@BudgetGroupId IS NULL OR A.BudgetGroupId = @BudgetGroupId)     
                        AND (@BudgetDate IS NULL OR @BudgetDate BETWEEN A.FromDate AND A.ToDate)                   
                        AND A.UnitId = @UnitId ORDER BY RM.SortOrder ASC;
                ";

            return (await _dbConnection.QueryAsync<GetSpindleMonthwiseReportDto>(sql,
                new { FinancialYearId = financialYearId, DepartmentId = departmentId, CostCenterId = costCenterId, UnitId = unitId, AllocationTypeId = allocationTypeId, BudgetDate = budgetDate, BudgetGroupId = budgetGroupId }))
                .ToList();
        }


     public async Task<RemainingBalanceWithPrevDto> GetRemainingBalanceAsync(
            int budgetGroupId,
            DateOnly? budgetDate,
            int? monthId,
            int? requestById,
            int? projectId,
            int? wbsId,int? financialYearId,
            CancellationToken ct = default)
        {
            var unitId = _ipAddressService.GetUnitId();

            // treat 0 as null
            projectId = (projectId.HasValue && projectId.Value <= 0) ? null : projectId;
            wbsId     = (wbsId.HasValue && wbsId.Value <= 0) ? null : wbsId;
            monthId   = (monthId.HasValue && monthId.Value <= 0) ? null : monthId;

            int? prevMonthId = monthId.HasValue ? monthId.Value - 1 : null;

            DateTime? sqlDate = budgetDate.HasValue
                ? budgetDate.Value.ToDateTime(TimeOnly.MinValue)
                : null;

            const string sql = @"
                -- Current
                SELECT TOP 1 Id, RemainingBalance
                FROM Budget.BudgetAllocation
                WHERE UnitId = @UnitId
                AND BudgetGroupId = @BudgetGroupId
                AND (@RequestById IS NULL OR RequestById = @RequestById)
                AND (@BudgetDate IS NULL OR @BudgetDate BETWEEN FromDate AND ToDate)
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
                AND (@BudgetDate IS NULL OR @BudgetDate BETWEEN FromDate AND ToDate)
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
                    MonthId = monthId,
                    PrevMonthId = prevMonthId,
                    RequestById = requestById,
                    BudgetDate = sqlDate,
                    ProjectId = projectId,
                    WbsId = wbsId,
                    FinancialYearId = financialYearId                    
                }, cancellationToken: ct));

            var currentRow = await multi.ReadFirstOrDefaultAsync<(int Id, decimal RemainingBalance)?>();
            var prevRow    = await multi.ReadFirstOrDefaultAsync<(int Id, decimal RemainingBalance)?>();

            return new RemainingBalanceWithPrevDto
            {
                BudgetGroupId = budgetGroupId,
                BudgetDate = budgetDate,
                MonthId = monthId,
                RequestById = requestById,
                ProjectId = projectId,
                WbsId = wbsId,

                Id = currentRow?.Id ?? 0,
                CurrentRemainingBalance = currentRow?.RemainingBalance,

                PreviousId = prevRow?.Id,
                PreviousRemainingBalance = prevRow?.RemainingBalance
            };
        }


        // ✅ Update using EF Core
        public async Task UpdateRemainingBalanceAsync(int id, decimal newRemainingBalance, CancellationToken ct = default)
        {
            var allocation = await _applicationDbContext.BudgetAllocations.AsNoTracking()
.FirstOrDefaultAsync(x => x.Id == id, ct);

            if (allocation == null) return;

            allocation.RemainingBalance = newRemainingBalance;
            await _applicationDbContext.SaveChangesAsync(ct);
        }

       public async Task<(int Id, decimal RemainingBalance)?> GetAllocationForBalanceAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int requestById,
            int? projectId,
            int? wbsId,
            CancellationToken ct = default)
        {
            projectId = (projectId.HasValue && projectId.Value <= 0) ? null : projectId;
            wbsId     = (wbsId.HasValue && wbsId.Value <= 0) ? null : wbsId;

            var sql = @"
                    SELECT TOP 1 Id, RemainingBalance
                    FROM Budget.BudgetAllocation
                    WHERE BudgetGroupId = @BudgetGroupId
                    AND RequestById   = @RequestById
                    AND FromDate     <= @BudgetDate
                    AND ToDate       >= @BudgetDate
                    AND (@ProjectId IS NULL OR ProjectId = @ProjectId)
                    AND (@WbsId     IS NULL OR WBSId     = @WbsId);
                    ";

            return await _dbConnection.QueryFirstOrDefaultAsync<(int Id, decimal RemainingBalance)?>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        BudgetGroupId = budgetGroupId,
                        BudgetDate    = budgetDate.ToDateTime(TimeOnly.MinValue),
                        RequestById   = requestById,
                        ProjectId     = projectId,
                        WbsId         = wbsId
                    },
                    cancellationToken: ct));
        }

        public async Task<List<BudgetBalanceReportDto>> GetBudgetAllocationsAsync(int financialYearId)
        {
            var unitId = _ipAddressService.GetUnitId();
            var sql = @"
                SELECT 
                    A.UnitId,
                    A.FinancialYearId,
                    A.RequestById,
                    B.Description AS RequestType,

                    A.RequestMonthId,
                    C.Code AS RequestMonth,

                    A.BudgetGroupId,
                    D.Name AS BudgetGroupName,

                    A.AllocationTypeId,
                    E.Description AS AllocationType,

                    A.ApprovedAmount,
                    A.RemainingBalance,

                    (ISNULL(A.ApprovedAmount,0) 
                     - ISNULL(A.RemainingBalance,0)) AS UtilizedAmount

                FROM Budget.BudgetAllocation A
                INNER JOIN Budget.MiscMaster B ON A.RequestById = B.Id
                INNER JOIN Budget.MiscMaster C ON A.RequestMonthId = C.Id
                INNER JOIN Budget.BudgetGroup D ON A.BudgetGroupId = D.Id
                INNER JOIN Budget.MiscMaster E ON A.AllocationTypeId = E.Id
                WHERE A.FinancialYearId = @FinancialYearId AND
                      A.UnitId = @UnitId
                ORDER BY A.BudgetGroupId ASC;
            ";

             var result = await _dbConnection.QueryAsync<BudgetBalanceReportDto>(
                sql,
                new
                {
                    UnitId = unitId,
                    FinancialYearId = financialYearId
                });

            return result.ToList();
        }
    }
}

    
