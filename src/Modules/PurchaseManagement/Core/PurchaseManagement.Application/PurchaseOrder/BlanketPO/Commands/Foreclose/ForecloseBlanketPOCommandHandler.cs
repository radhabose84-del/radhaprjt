using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBlanketPO;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.BlanketPO.Commands.Foreclose;

public sealed class ForecloseBlanketPOCommandHandler
    : IRequestHandler<ForecloseBlanketPOCommand, bool>
{
    private readonly IBlanketPOCommandRepository _commandRepo;
    private readonly IMediator _mediator;

    public ForecloseBlanketPOCommandHandler(
        IBlanketPOCommandRepository commandRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(ForecloseBlanketPOCommand request, CancellationToken ct)
    {
        var result = await _commandRepo.ForecloseAsync(request.Id, ct);
        if (!result)
            throw new ExceptionRules("Blanket Release PO not found or already foreclosed.");

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Foreclose",
            actionCode: "BLANKET_RELEASE_PO_FORECLOSE",
            actionName: request.Id.ToString(),
            details: $"Blanket Release PO with Id {request.Id} foreclosed successfully.",
            module: "BlanketPO"
        );
        await _mediator.Publish(ev, ct);

        return true;
    }
}
