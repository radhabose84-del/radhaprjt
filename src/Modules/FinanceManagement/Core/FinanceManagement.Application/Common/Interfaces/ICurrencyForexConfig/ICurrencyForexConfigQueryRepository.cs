using FinanceManagement.Application.CurrencyForexConfig.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig
{
    public interface ICurrencyForexConfigQueryRepository
    {
        Task<(List<CurrencyForexConfigDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int companyId);
        Task<CurrencyForexConfigDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<CurrencyForexConfigLookupDto>> AutocompleteAsync(string term, int companyId, CancellationToken ct);

        Task<bool> AlreadyExistsByCodeAsync(string currencyTypeCode, int companyId, int? id = null);
        Task<bool> AlreadyExistsByNameAsync(string currencyTypeName, int companyId, int? id = null);
        Task<bool> NotFoundAsync(int id);

        // Rule 25 — block delete / inactivate when GL accounts reference this config
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsCurrencyForexConfigLinkedAsync(int id);
    }
}
