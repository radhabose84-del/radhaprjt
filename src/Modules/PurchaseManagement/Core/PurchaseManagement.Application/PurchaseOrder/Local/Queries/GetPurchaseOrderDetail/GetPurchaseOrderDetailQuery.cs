using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderDetail;

/// <summary>
/// PO Analysis detail — all-PO-types line detail plus the Outstanding / Budget / Approval / PO Progress
/// summary panels, in a single response.
/// </summary>
public record GetPurchaseOrderDetailQuery(int Id) : IRequest<PurchaseOrderDetailWithSummaryDto?>;
