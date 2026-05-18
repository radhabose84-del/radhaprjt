using PurchaseManagement.Application.ContractPO.Dto;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;

public interface IContractPOQueryRepository
{
    Task<ContractPOHeaderDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<(IReadOnlyList<ContractPOHeaderDto> Items, int Total)> GetAllAsync(
        int page, int size, string? search, CancellationToken ct);
    Task<IReadOnlyList<ContractPOLookupDto>> AutocompleteAsync(string term, CancellationToken ct);
    Task<bool> NotFoundAsync(int id, CancellationToken ct);
    Task<bool> AlreadyExistsAsync(string contractPONumber, int? excludeId = null);
    Task<bool> HasReleaseHistoryAsync(int id);
    Task<bool> SoftDeleteValidationAsync(int id);

    // Combine PO validation
    Task<bool> IsContractActiveAndValidAsync(int contractPOHeaderId);
    Task<decimal> GetContractDetailBalanceAsync(int contractPODetailId);

    // Contract Release PO (Combine PO) queries
    Task<ContractReleasePODetailVm?> GetContractReleasePOByIdAsync(int poId, CancellationToken ct);
}
