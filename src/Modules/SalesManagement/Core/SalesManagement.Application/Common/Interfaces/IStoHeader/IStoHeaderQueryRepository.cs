using SalesManagement.Application.StoHeader.Dto;

namespace SalesManagement.Application.Common.Interfaces.IStoHeader
{
    public interface IStoHeaderQueryRepository
    {
        Task<(List<StoHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<StoHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<StoHeaderLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> StoTypeExistsAsync(int id);
        Task<bool> MovementTypeExistsAsync(int id);
        Task<(List<StoHeaderDto>, int)> GetPendingAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<StoHeaderDto?> GetPendingByIdAsync(int id);
    }
}
