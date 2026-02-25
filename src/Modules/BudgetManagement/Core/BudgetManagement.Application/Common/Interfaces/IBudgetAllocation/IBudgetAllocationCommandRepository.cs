namespace BudgetManagement.Application.Common.Interfaces.IBudgetAllocation
{
    public interface IBudgetAllocationCommandRepository
    {
        Task<int> CreateAsync(BudgetManagement.Domain.Entities.BudgetAllocation budgetAllocation);
        Task<bool> UpdateRemainingBalanceAsync(int id, decimal newRemainingBalance, CancellationToken ct = default);        
        Task<BudgetManagement.Domain.Entities.BudgetAllocation?> GetByKeyAsync(int unitId,int financialYearId,int? requestMonthId,int? budgetGroupId,int requestById,DateOnly fromDate,DateOnly toDate, int? wbsId,int? projectId,  CancellationToken ct = default);
    }
}