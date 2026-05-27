using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.DeleteReturnReason;

public sealed class DeleteReturnReasonCommandHandler : IRequestHandler<DeleteReturnReasonCommand, bool>
{
    private readonly IReturnReasonCommandRepository _commandRepo;
    private readonly IMediator _mediator;

    public DeleteReturnReasonCommandHandler(
        IReturnReasonCommandRepository commandRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeleteReturnReasonCommand request, CancellationToken ct)
    {
        var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
        if (!ok)
            throw new ExceptionRules("ReturnReason not found.");

        var ev = new AuditLogsDomainEvent(
            actionDetail: "SoftDelete",
            actionCode: "RETURNREASON_DELETE",
            actionName: request.Id.ToString(),
            details: $"ReturnReason with Id {request.Id} soft-deleted.",
            module: "ReturnReason");
        await _mediator.Publish(ev, ct);

        return true;
    }
}
