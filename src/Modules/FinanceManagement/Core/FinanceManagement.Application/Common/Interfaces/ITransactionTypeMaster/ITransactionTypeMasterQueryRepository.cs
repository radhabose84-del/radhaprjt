using FinanceManagement.Application.TransactionTypeMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster
{
    public interface ITransactionTypeMasterQueryRepository
    {
        Task<(List<TransactionTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<TransactionTypeMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<TransactionTypeMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> TypeNameExistsAsync(string typeName, int? id = null);
        Task<bool> ShortNameExistsAsync(string shortName, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> UnitExistsAsync(int unitId);
        Task<bool> ModuleExistsAsync(int moduleId);
        Task<bool> MenuExistsAsync(int menuId);
    }
}
