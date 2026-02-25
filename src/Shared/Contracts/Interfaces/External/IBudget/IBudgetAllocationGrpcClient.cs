using Contracts.Dtos.Budget;

namespace Contracts.Interfaces.External.IBudget
{
    public interface IBudgetAllocationGrpcClient
    {
        Task<List<BudgetAllocationDto>> GetSpindleDetailsMonthwiseAsync(
            int financialYearId,
            int? departmentId = null,
            int? costCenterId = null,
            int? allocationTypeId = null,
            int? budgetGroupId = null,
            DateOnly? budgetDate = null,
            CancellationToken ct = default);

        Task<bool> ApplyRemainingBalanceDeltaAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int monthId,
            int requestById,
            decimal deltaAmount,
            int? projectId,
            int? wbsId,int? financialYearId,
            CancellationToken ct = default);
        
         Task<RemainingBalanceWithPrevDto?> GetRemainingBalanceAsync(
            int budgetGroupId,
            DateOnly budgetDate,
            int monthId ,
            int requestById ,
            int? projectId ,
            int? wbsId , int? financialYearId ,
            CancellationToken ct = default);
    }
}
