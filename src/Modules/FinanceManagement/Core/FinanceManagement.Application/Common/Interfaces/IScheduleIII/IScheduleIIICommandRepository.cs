using FinanceManagement.Domain.Entities;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    public interface IScheduleIIICommandRepository
    {
        // Header = one row per (Company, Division). EnsureHeader creates a DRAFT header if none exists.
        Task<int> EnsureHeaderAsync(int companyId, int divisionId);
        Task<int> UpdateHeaderAsync(int companyId, int divisionId, int statusId, bool textileSplitEnabled);
        Task<bool> LockStructureAsync(int companyId, int divisionId);

        // Detail = the included lines of a header.
        Task<int> CreateDetailAsync(ScheduleIIIDetail entity);
        Task<int> UpdateDetailAsync(ScheduleIIIDetail entity);
        Task<bool> SoftDeleteDetailAsync(int id, CancellationToken ct);
        Task<bool> ReorderDetailAsync(int detailId, int direction, CancellationToken ct);

        // Section (global catalog)
        Task<int> CreateSectionAsync(ScheduleIIISection entity);
        Task<int> UpdateSectionAsync(ScheduleIIISection entity);

        // Line items (global catalog)
        Task<int> CreateLineItemAsync(ScheduleIIISectionItem entity);
        Task<int> UpdateLineItemAsync(ScheduleIIISectionItem entity);
        Task<bool> SoftDeleteLineItemAsync(int id, CancellationToken ct);

        // Sub-total HEADERS (catalog of formulas: Gross Profit / EBITDA / PBT / PAT) — header fields only.
        Task<int> CreateSubTotalAsync(ScheduleIIISubTotal subTotal);
        Task<int> UpdateSubTotalAsync(ScheduleIIISubTotal subTotal);
        Task<bool> SoftDeleteSubTotalAsync(int id, CancellationToken ct);

        // Sub-total formula operands (separate ScheduleIIISubTotalFormula API) — physical replace, logged to ActivityLog.
        Task<int> SaveSubTotalFormulaAsync(int subTotalId, List<ScheduleIIISubTotalFormula> formulas);
    }
}
