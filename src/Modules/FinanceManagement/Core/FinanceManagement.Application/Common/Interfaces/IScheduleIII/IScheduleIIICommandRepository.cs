using FinanceManagement.Domain.Entities;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    public interface IScheduleIIICommandRepository
    {
        // Line items
        Task<int> CreateLineItemAsync(ScheduleIIILineItem entity);
        Task<int> UpdateLineItemAsync(ScheduleIIILineItem entity);
        Task<bool> SoftDeleteLineItemAsync(int id, CancellationToken ct);

        // Reorder: direction 1 = move up, 2 = move down (swaps DisplayOrder with neighbour in same section)
        Task<bool> ReorderLineItemAsync(int lineItemId, int direction, CancellationToken ct);

        // Sub-totals (+ formula operands)
        Task<int> CreateSubTotalAsync(ScheduleIIISubTotal subTotal, List<ScheduleIIISubTotalFormula> formulas);
        Task<int> UpdateSubTotalAsync(int subTotalId, string? subTotalName, bool includeOtherIncome, List<ScheduleIIISubTotalFormula> formulas);

        // Structure-level
        Task<bool> LockStructureAsync(int structureId);
    }
}
