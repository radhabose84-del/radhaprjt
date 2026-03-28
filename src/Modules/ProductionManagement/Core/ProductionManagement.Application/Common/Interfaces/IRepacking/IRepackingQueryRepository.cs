using ProductionManagement.Application.Repacking.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IRepacking
{
    public interface IRepackingQueryRepository
    {
        Task<(List<RepackingHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<RepackingHeaderDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<RepackingLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> OldPackHeaderExistsAsync(int oldPackHeaderId);
        Task<bool> PackDetailExistsAsync(int oldPackDetailId);
        Task<bool> LotExistsAsync(int lotId);
        Task<bool> PackTypeExistsAsync(int packTypeId);
    }
}
