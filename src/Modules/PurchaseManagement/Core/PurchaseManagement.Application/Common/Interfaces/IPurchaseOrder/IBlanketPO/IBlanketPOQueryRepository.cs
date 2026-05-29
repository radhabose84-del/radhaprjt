using PurchaseManagement.Application.PurchaseOrder.Dtos.BlanketPO;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;

public interface IBlanketPOQueryRepository
{
    Task<decimal> GetBlanketDetailBalanceAsync(int blanketDetailId);
    Task<BlanketPODetailVm?> GetBlanketPOByIdAsync(int poId, CancellationToken ct);
    Task<bool> HasAnyGrnAsync(int poId, CancellationToken ct);
    Task<(List<GetBlanketPOPendingGroupDto> Rows, int Total)> GetBlanketPOPendingAsync(
        int? page, int? size, string? search, int? poId, CancellationToken ct);
    Task<bool> NotFoundAsync(int poId, CancellationToken ct);
}
