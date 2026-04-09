using Contracts.Dtos.Lookups.Sales;
using SalesManagement.Application.CommissionSplit.Dto;

namespace SalesManagement.Application.Common.Interfaces.ICommissionSplit
{
    public interface ICommissionSplitQueryRepository
    {
        Task<(List<CommissionSplitDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<CommissionSplitDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<CommissionSplitLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string splitName, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> MiscMasterExistsAsync(int id);
        Task<string?> GetMiscMasterCodeAsync(int id);
    }
}
