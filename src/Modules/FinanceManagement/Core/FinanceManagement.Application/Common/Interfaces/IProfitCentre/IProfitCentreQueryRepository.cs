using FinanceManagement.Application.ProfitCentre.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IProfitCentre
{
    public interface IProfitCentreQueryRepository
    {
        Task<(List<ProfitCentreDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<ProfitCentreDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<ProfitCentreLookupDto>> AutocompleteAsync(string term, int? levelId, CancellationToken ct);

        // Uniqueness is global — a PC code may not be reused in any company (AC#2).
        Task<bool> AlreadyExistsByCodeAsync(string profitCentreCode, int? id = null);
        Task<bool> NotFoundAsync(int id);

        // Hierarchy helpers — ordinal comes from the stable MiscMaster.SortOrder (1/2), never the level Id.
        Task<int> GetLevelSortOrderAsync(int levelId);
        Task<bool> ParentValidForLevelAsync(int? parentProfitCentreId, int levelId);

        // Delete/deactivate guards. Children check is real; current-year-transaction check is a stub (false)
        // until the journal engine tags transactions to profit centres (AC#5).
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> HasCurrentYearTransactionsAsync(int profitCentreId);
    }
}
