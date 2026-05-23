using PurchaseManagement.Application.PurchaseOrder.Print.Dto;

namespace PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;

public interface IPurchaseOrderPrintQueryRepository
{
    Task<PurchaseOrderPrintDto?> GetPrintDetailsAsync(int purchaseOrderId, CancellationToken ct = default);
}
