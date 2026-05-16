using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.Local.Commands.Foreclose;

public class ForeclosePurchaseOrderCommandHandler : IRequestHandler<ForeclosePurchaseOrderCommand, bool>
{
    private readonly IPurchaseOrderCommandRepository _commandRepository;
    private readonly IMediator _mediator;

    public ForeclosePurchaseOrderCommandHandler(
        IPurchaseOrderCommandRepository commandRepository,
        IMediator mediator)
    {
        _commandRepository = commandRepository;
        _mediator = mediator;
    }

    public async Task<bool> Handle(ForeclosePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await _commandRepository.ForecloseAsync(request.Id, cancellationToken);

        if (!result)
            throw new ExceptionRules("Purchase Order not found.");

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Foreclose",
            actionCode: "PURCHASEORDER_FORECLOSE",
            actionName: request.Id.ToString(),
            details: $"Purchase Order with Id {request.Id} foreclosed successfully.",
            module: "PurchaseOrder");
        await _mediator.Publish(auditEvent, cancellationToken);

        return true;
    }
}
