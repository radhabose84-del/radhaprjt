using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Delete;

public sealed class DeleteContractPOCommandHandler
    : IRequestHandler<DeleteContractPOCommand, bool>
{
    private readonly IContractPOCommandRepository _commandRepo;
    private readonly IMediator _mediator;

    public DeleteContractPOCommandHandler(
        IContractPOCommandRepository commandRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeleteContractPOCommand request, CancellationToken ct)
    {
        var result = await _commandRepo.DeleteContractPOAsync(request.Id, ct);

        if (result > 0)
        {
            var ev = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "CONTRACT_RELEASE_PO_DELETE",
                actionName: request.Id.ToString(),
                details: $"Contract Release PO with Id {request.Id} deleted successfully.",
                module: "ContractPO"
            );
            await _mediator.Publish(ev, ct);
        }

        return result > 0;
    }
}
