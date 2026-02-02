using MediatR;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Delete;

public record DeletePurchaseOrderCommand : IRequest<bool>
{
    public required int Id { get; init; }
}
