using FinanceManagement.Domain.Entities;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    public interface IScheduleIIICommandRepository
    {
        // Master = one row per included line (header + line link). Structure = rows sharing (Company, Division).
        Task<int> CreateMasterAsync(ScheduleIIIMaster entity);
        Task<int> UpdateMasterAsync(ScheduleIIIMaster entity);
        Task<bool> SoftDeleteMasterAsync(int id, CancellationToken ct);
        Task<bool> ReorderMasterAsync(int masterId, int direction, CancellationToken ct);
        Task<bool> LockStructureAsync(int scheduleIIIMasterId);

        // Section (global catalog)
        Task<int> CreateSectionAsync(ScheduleIIISection entity);
        Task<int> UpdateSectionAsync(ScheduleIIISection entity);

        // Line items (global catalog)
        Task<int> CreateLineItemAsync(ScheduleIIISectionItem entity);
        Task<int> UpdateLineItemAsync(ScheduleIIISectionItem entity);
        Task<bool> SoftDeleteLineItemAsync(int id, CancellationToken ct);

        // Sub-totals (+ formula operands)
        Task<int> CreateSubTotalAsync(ScheduleIIISubTotal subTotal, List<ScheduleIIISubTotalFormula> formulas);
        Task<int> UpdateSubTotalAsync(int subTotalId, string? formulaName, bool includeOtherIncome, List<ScheduleIIISubTotalFormula> formulas);
    }
}
