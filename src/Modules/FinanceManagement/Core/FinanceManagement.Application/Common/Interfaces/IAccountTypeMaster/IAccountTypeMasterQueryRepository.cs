using FinanceManagement.Application.AccountTypeMaster.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster
{
    public interface IAccountTypeMasterQueryRepository
    {
        Task<(List<AccountTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null);
        Task<AccountTypeMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<AccountTypeMasterLookupDto>> AutocompleteAsync(string term, int? companyId, CancellationToken ct);
        Task<bool> AlreadyExistsByNameAsync(string accountTypeName, int companyId, int? id = null);
        Task<bool> AlreadyExistsByStartCodeAsync(string startCode, int companyId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsAccountTypeLinkedAsync(int id);
    }
}
