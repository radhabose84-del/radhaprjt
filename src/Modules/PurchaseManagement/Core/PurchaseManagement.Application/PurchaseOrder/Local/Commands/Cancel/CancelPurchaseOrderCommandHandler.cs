using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Cancel;

public class CancelPurchaseOrderCommandHandler : IRequestHandler<CancelPurchaseOrderCommand, bool>
{
    private readonly IPurchaseOrderCommandRepository _commandRepository;
    private readonly IMediator _mediator;

    public CancelPurchaseOrderCommandHandler(
        IPurchaseOrderCommandRepository commandRepository,
        IMediator mediator)
    {
        _commandRepository = commandRepository;
        _mediator = mediator;
    }

    public async Task<bool> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await _commandRepository.CancelAsync(request.Id, cancellationToken);

        if (!result)
            throw new ExceptionRules("Purchase Order not found.");

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Cancel",
            actionCode: "PURCHASEORDER_CANCEL",
            actionName: request.Id.ToString(),
            details: $"Purchase Order with Id {request.Id} cancelled successfully.",
            module: "PurchaseOrder");
        await _mediator.Publish(auditEvent, cancellationToken);

        return true;
    }
}
