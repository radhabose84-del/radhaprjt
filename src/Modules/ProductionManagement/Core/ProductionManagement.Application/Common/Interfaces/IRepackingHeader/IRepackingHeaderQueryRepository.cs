using ProductionManagement.Application.RepackingHeader.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IRepackingHeader
{
    public interface IRepackingHeaderQueryRepository
    {
        Task<(List<RepackingHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<RepackingHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<RepackingHeaderLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> PackTypeExistsAsync(int id);
        Task<bool> MiscMasterExistsAsync(int id);
        Task<bool> LotMasterExistsAsync(int id);
    }
}
