using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Cancel;

public sealed class CancelBlanketPOCommandHandler
    : IRequestHandler<CancelBlanketPOCommand, bool>
{
    private readonly IBlanketPOCommandRepository _commandRepo;
    private readonly IMediator _mediator;

    public CancelBlanketPOCommandHandler(
        IBlanketPOCommandRepository commandRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(CancelBlanketPOCommand request, CancellationToken ct)
    {
        var result = await _commandRepo.CancelAsync(request.Id, ct);
        if (!result)
            throw new ExceptionRules("Blanket Release PO not found or already cancelled.");

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Cancel",
            actionCode: "BLANKET_RELEASE_PO_CANCEL",
            actionName: request.Id.ToString(),
            details: $"Blanket Release PO with Id {request.Id} cancelled successfully.",
            module: "BlanketPO"
        );
        await _mediator.Publish(ev, ct);

        return true;
    }
}
