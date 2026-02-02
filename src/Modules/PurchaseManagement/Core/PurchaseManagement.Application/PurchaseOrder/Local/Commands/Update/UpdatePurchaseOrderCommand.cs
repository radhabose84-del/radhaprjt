// using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using MediatR;
using PurchaseLocalDetailDto.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Update;
public record UpdatePurchaseOrderCommand : IRequest<bool>
{
    public required PurchaseOrderUpdateDto Data { get; init; }
}

