using MediatR;
using PurchaseLocalDetailDto.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Queries.GetPurchaseOrderById;

public record GetPurchaseOrderByIdQuery(int Id) : IRequest<PurchaseOrderDetailDto?>;