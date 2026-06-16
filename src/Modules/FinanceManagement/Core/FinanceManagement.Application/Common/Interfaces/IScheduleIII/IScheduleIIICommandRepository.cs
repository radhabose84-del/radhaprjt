using FinanceManagement.Application.ScheduleIII.Dto;
using FinanceManagement.Domain.Entities;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    public interface IScheduleIIICommandRepository
    {
        // Composite aggregate — one call persists Structure + Sections + LineItems + SubTotals + Formulas.
        Task<int> CreateAggregateAsync(ScheduleIIIInput input);
        Task<int> UpdateAggregateAsync(ScheduleIIIInput input);

        // Structure
        Task<bool> LockStructureAsync(int structureId);

        // Line items (granular)
        Task<int> CreateLineItemAsync(ScheduleIIILineItem entity);
        Task<int> UpdateLineItemAsync(ScheduleIIILineItem entity);
        Task<bool> SoftDeleteLineItemAsync(int id, CancellationToken ct);
        Task<bool> ReorderLineItemAsync(int lineItemId, int direction, CancellationToken ct);

        // Sub-totals (granular, + formula operands)
        Task<int> CreateSubTotalAsync(ScheduleIIISubTotal subTotal, List<ScheduleIIISubTotalFormula> formulas);
        Task<int> UpdateSubTotalAsync(int subTotalId, string? subTotalName, bool includeOtherIncome, List<ScheduleIIISubTotalFormula> formulas);
    }
}
