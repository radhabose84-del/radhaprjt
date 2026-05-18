using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.ContractPO.Commands.Delete;

public sealed class DeleteContractPOCommandHandler
    : IRequestHandler<DeleteContractPOCommand, bool>
{
    private readonly IContractPOCommandRepository _commandRepo;
    private readonly IContractPOQueryRepository _queryRepo;
    private readonly IMediator _mediator;

    public DeleteContractPOCommandHandler(
        IContractPOCommandRepository commandRepo,
        IContractPOQueryRepository queryRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeleteContractPOCommand request, CancellationToken ct)
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
