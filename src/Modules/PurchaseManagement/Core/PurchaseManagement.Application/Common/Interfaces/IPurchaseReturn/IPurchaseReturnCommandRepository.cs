using PurchaseManagement.Domain.Entities.PurchaseReturn;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;

public interface IPurchaseReturnCommandRepository
{
    Task<PurchaseReturnHeader> CreateAsync(PurchaseReturnHeader entity, int transactionTypeId, CancellationToken ct);
    Task<PurchaseReturnHeader> UpdateAsync(PurchaseReturnHeader entity, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    Task<bool> SetStatusAsync(int id, int statusId, CancellationToken ct);
    Task<bool> SetApprovalRequestIdAsync(int id, int approvalRequestId, CancellationToken ct);
    Task WriteStockLedgerOnApprovalAsync(int purchaseReturnHeaderId, CancellationToken ct);
}
