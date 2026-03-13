using FinanceManagement.Application.EWaybillHeader.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IEWaybillHeader
{
    public interface IEWaybillHeaderQueryRepository
    {
        Task<(List<EWaybillHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<EWaybillHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<EWaybillHeaderLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> EWBNumberExistsAsync(string ewbNumber, int? excludeId = null);
        Task<bool> NotFoundAsync(int id);
    }
}
