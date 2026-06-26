using FinanceManagement.Application.ScheduleIII.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IScheduleIII
{
    public interface IScheduleIIIImportQueryRepository
    {
        // MiscMaster options for resolving the sheet's StatementType / Nature text (by code or description).
        Task<IReadOnlyList<ScheduleIIIMiscOptionDto>> GetStatementTypeOptionsAsync();
        Task<IReadOnlyList<ScheduleIIIMiscOptionDto>> GetNatureOptionsAsync();

        // Section names that already exist (non-deleted) — used to block duplicate creation.
        Task<IReadOnlyList<string>> GetExistingSectionNamesAsync(IEnumerable<string> names);

        // Highest DisplayOrder among existing (non-deleted) sections — imported sections are sequenced after it.
        Task<int> GetMaxSectionDisplayOrderAsync();
    }
}
