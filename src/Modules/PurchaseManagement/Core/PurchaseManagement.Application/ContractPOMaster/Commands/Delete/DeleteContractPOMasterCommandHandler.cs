using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.ContractPOMaster.Commands.Delete;

public sealed class DeleteContractPOMasterCommandHandler
    : IRequestHandler<DeleteContractPOMasterCommand, bool>
{
    private readonly IContractPOMasterCommandRepository _commandRepo;
    private readonly IContractPOMasterQueryRepository _queryRepo;
    private readonly IMediator _mediator;

    public DeleteContractPOMasterCommandHandler(
        IContractPOMasterCommandRepository commandRepo,
        IContractPOMasterQueryRepository queryRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeleteContractPOMasterCommand request, CancellationToken ct)
    {
        // Fetch for audit text before deletion
        var before = await _queryRepo.GetByIdAsync(request.Id, ct)
                     ?? throw new ExceptionRules("Contract PO not found.");

        var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
        if (!ok) throw new ExceptionRules("Failed to delete Contract PO.");

        // Audit
        var ev = new AuditLogsDomainEvent(
            actionDetail: "SoftDelete",
            actionCode: before.ContractPONumber ?? request.Id.ToString(),
            actionName: $"Contract PO {before.ContractPONumber}",
            details: $"Contract PO '{before.ContractPONumber}' with Id {request.Id} soft-deleted.",
            module: "ContractPO"
        );
        await _mediator.Publish(ev, ct);

        return true;
    }
}
