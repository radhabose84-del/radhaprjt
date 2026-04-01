using Contracts.Dtos.Stock;
using ProductionManagement.Application.RepackingMaster.Dto;

namespace ProductionManagement.Application.Common.Interfaces.IRepackingMaster
{
    public interface IRepackingMasterQueryRepository
    {
        Task<(List<RepackingMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<RepackingMasterDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<RepackingMasterLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> AlreadyExistsAsync(string repackDocNo, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> PackTypeExistsAsync(int packTypeId);
        Task<bool> MiscMasterExistsAsync(int miscMasterId);
    }
}
