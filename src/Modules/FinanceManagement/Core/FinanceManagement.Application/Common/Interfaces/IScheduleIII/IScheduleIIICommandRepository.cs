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

        // Sub-totals (catalog of formulas: Gross Profit / EBITDA / PBT / PAT) — operands persisted together.
        // Update replaces the operand set (old rows hard-deleted, logged to ActivityLog).
        Task<int> CreateSubTotalAsync(ScheduleIIISubTotal subTotal, List<ScheduleIIISubTotalFormula> formulas);
        Task<int> UpdateSubTotalAsync(int subTotalId, string? formulaName, bool includeOtherIncome, List<ScheduleIIISubTotalFormula> formulas);
        Task<bool> SoftDeleteSubTotalAsync(int id, CancellationToken ct);

        // Sub-total formula operands (separate ScheduleIIISubTotalFormula API) — physical replace, logged to ActivityLog.
        Task<int> SaveSubTotalFormulaAsync(int subTotalId, List<ScheduleIIISubTotalFormula> formulas);
    }
}
