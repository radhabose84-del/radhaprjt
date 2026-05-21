using PurchaseManagement.Application.PurchaseOrder.ContractPO.Queries.GetContractPOPending;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ContractPO;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;

public interface IContractPOQueryRepository
{
    Task<bool> IsContractActiveAndValidAsync(int contractPOHeaderId);
    Task<decimal> GetContractDetailBalanceAsync(int contractPODetailId);
    Task<ContractPODetailVm?> GetContractPOByIdAsync(int poId, CancellationToken ct);
    Task<bool> HasAnyGrnAsync(int poId, CancellationToken ct);
    Task<(List<GetContractPOPendingGroupDto> Rows, int Total)> GetContractPOPendingAsync(
        int? page, int? size, string? search, int? poId, CancellationToken ct);
    Task<bool> NotFoundAsync(int poId, CancellationToken ct);
}
