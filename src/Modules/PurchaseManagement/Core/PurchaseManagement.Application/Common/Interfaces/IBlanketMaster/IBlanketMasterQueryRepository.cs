using PurchaseManagement.Application.BlanketMaster.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;

public interface IBlanketMasterQueryRepository
{
    Task<(List<BlanketHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, CancellationToken ct);
    Task<BlanketHeaderDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<BlanketMasterLookupDto>> AutocompleteAsync(string term, bool approvedOnly, int? vendorId, DateTimeOffset? poDate, CancellationToken ct);
    Task<bool> NotFoundAsync(int id, CancellationToken ct);
    Task<bool> AlreadyExistsAsync(string blanketNumber, int? excludeId = null);
    Task<(List<GetBlanketMasterPendingGroupDto>, int)> GetBlanketMasterPendingAsync(int page, int size, string? search, CancellationToken ct);
    Task<decimal> GetPendingQuantityAsync(int blanketDetailId, CancellationToken ct);
    Task<bool> IsApprovedAsync(int id, CancellationToken ct);
    Task<bool> IsExpiredAsync(int id, CancellationToken ct);
    Task<bool> HasOverlappingBlanketAsync(int vendorId, List<int> itemIds, DateTimeOffset validityFrom, DateTimeOffset validityTo, int? excludeId = null);
}
