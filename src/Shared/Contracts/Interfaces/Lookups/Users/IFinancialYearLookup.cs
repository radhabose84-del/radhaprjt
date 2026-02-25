using Contracts.Dtos.Lookups.Users;

namespace Contracts.Interfaces.Lookups.Users
{
    public interface IFinancialYearLookup
    {
        Task<List<FinancialYearLookupDto>> GetAllFinancialYearAsync();
        Task<FinancialYearLookupDto?> GetByIdAsync(int financialYearId, CancellationToken ct = default);
        Task<IReadOnlyList<FinancialYearLookupDto>> GetByIdsAsync(IEnumerable<int> financialYearIds, CancellationToken ct = default);
    }
}
