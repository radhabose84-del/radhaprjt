using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;

public interface IImportPOQueryRepository
{
    Task<ImportPOFullVm?> GetByIdAsync(int id, CancellationToken ct);
    Task<(List<GetPOImportPendingGroupDto> Rows, int Total)> GetImportPOPendingAsync(
        int? page, int? size, string? search, int? poId, CancellationToken ct);
    Task<bool> HasAnyGrnAsync(int poId, CancellationToken ct);
    Task<bool> ExistsAsync(int poId, CancellationToken ct);
    Task<string?> GetStatusCodeAsync(int poId, CancellationToken ct);    
}

