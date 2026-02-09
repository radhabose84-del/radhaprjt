using MediatR;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderById;

public record GetPurchaseOrderByIdQuery(int Id) : IRequest<PurchaseOrderDetailDto?>;