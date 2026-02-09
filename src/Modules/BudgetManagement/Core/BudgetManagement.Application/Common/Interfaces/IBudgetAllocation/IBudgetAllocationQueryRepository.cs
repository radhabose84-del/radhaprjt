using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BudgetManagement.Application.BudgetAllocation.Queries.GetBudgetBalanceReport;
using BudgetManagement.Application.BudgetAllocation.Queries.GetRemainingBalance;
using BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleDetailsMonthwise;
using BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleMonthwiseReport;

namespace BudgetManagement.Application.Common.Interfaces.IBudgetAllocation
{
  public interface IBudgetAllocationQueryRepository
  {
    Task<(List<GetSpindleDetailsMonthwiseDto>, int)> GetBudgetGroupDetailSpindlewise(int PageNumber, int PageSize, string? SearchTerm);
    Task<bool> ExistsAsync(
            int unitId,
            int financialYearId,
            int requestById,
            int requestMonthId,
            int budgetGroupId,
            int allocationTypeId,
            CancellationToken ct = default);

    Task<List<GetSpindleMonthwiseReportDto>> GetSpindleDetailsMonthwiseAsync(
                     int financialYearId, int? departmentId, int? costCenterId, int? allocationTypeId, int? budgetGroupId, DateOnly? BudgetDate, CancellationToken ct = default);

    Task<RemainingBalanceWithPrevDto> GetRemainingBalanceAsync(int budgetGroupId, DateOnly? budgetDate, int? monthId, int? requestById, int? projectId, int? wbsId, int? financialYearId, CancellationToken ct = default);

    Task UpdateRemainingBalanceAsync(int id, decimal newRemainingBalance, CancellationToken ct = default);   
     Task<List<BudgetBalanceReportDto>> GetBudgetAllocationsAsync(int financialYearId);
    }

}