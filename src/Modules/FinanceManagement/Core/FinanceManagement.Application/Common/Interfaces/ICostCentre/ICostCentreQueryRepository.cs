using FinanceManagement.Application.CostCentre.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ICostCentre
{
    public interface ICostCentreQueryRepository
    {
        Task<(List<CostCentreDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int unitId);
        Task<CostCentreDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<CostCentreLookupDto>> AutocompleteAsync(string term, int unitId, int? centreLevelId, CancellationToken ct);

        // Uniqueness is per unit — the same code may exist in a different unit.
        Task<bool> AlreadyExistsByCodeAsync(string costCentreCode, int unitId, int? id = null);
        Task<bool> NotFoundAsync(int id);

        // Hierarchy helpers — ordinal comes from the stable MiscMaster.SortOrder (1/2/3), never the level Id.
        Task<int> GetCentreLevelSortOrderAsync(int centreLevelId);
        Task<bool> ParentValidForLevelAsync(int? parentCostCentreId, int centreLevelId, int unitId);
        Task<bool> PlantExistsForUnitAsync(int unitId);

        // Delete/deactivate guards. Children check is real; open-transaction check is a stub (false)
        // until the journal engine tags transactions to cost centres (AC#3 / Sprint 2).
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> HasOpenTransactionsAsync(int costCentreId);
    }
}
