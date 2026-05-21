using PurchaseManagement.Application.ContractPOMaster.Dto;
using PurchaseManagement.Application.ContractPOMaster.Queries.GetPending;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;

public interface IContractPOMasterQueryRepository
{
    Task<ContractPOHeaderDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(IReadOnlyList<ContractPOHeaderDto> Items, int Total)> GetAllAsync(
        int page, int size, string? search, CancellationToken ct);
    Task<IReadOnlyList<ContractPOLookupDto>> AutocompleteAsync(string term, bool approvedOnly, int? vendorId, CancellationToken ct);
    Task<bool> NotFoundAsync(int id, CancellationToken ct);
    Task<bool> AlreadyExistsAsync(string contractPONumber, int? excludeId = null);
    Task<bool> HasReleaseHistoryAsync(int id);
    Task<bool> SoftDeleteValidationAsync(int id);
    Task<(List<GetContractPOMasterPendingGroupDto> Items, int Total)> GetContractPOMasterPendingAsync(
        int page, int size, string? search, CancellationToken ct);
}
