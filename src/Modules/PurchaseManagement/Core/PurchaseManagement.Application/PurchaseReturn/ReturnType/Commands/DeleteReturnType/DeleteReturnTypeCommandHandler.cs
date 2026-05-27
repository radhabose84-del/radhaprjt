using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.DeleteReturnType;

public sealed class DeleteReturnTypeCommandHandler : IRequestHandler<DeleteReturnTypeCommand, bool>
{
    private readonly IReturnTypeCommandRepository _commandRepo;
    private readonly IMediator _mediator;

    public DeleteReturnTypeCommandHandler(
        IReturnTypeCommandRepository commandRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeleteReturnTypeCommand request, CancellationToken ct)
    {
        var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
        if (!ok)
            throw new ExceptionRules("ReturnType not found.");

        var ev = new AuditLogsDomainEvent(
            actionDetail: "SoftDelete",
            actionCode: "RETURNTYPE_DELETE",
            actionName: request.Id.ToString(),
            details: $"ReturnType with Id {request.Id} soft-deleted.",
            module: "ReturnType");
        await _mediator.Publish(ev, ct);

        return true;
    }
}
