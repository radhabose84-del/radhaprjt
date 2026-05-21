using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Cancel;

public class CancelContractPOCommandHandler : IRequestHandler<CancelContractPOCommand, bool>
{
    private readonly IContractPOCommandRepository _commandRepository;
    private readonly IMediator _mediator;

    public CancelContractPOCommandHandler(
        IContractPOCommandRepository commandRepository,
        IMediator mediator)
    {
        _commandRepository = commandRepository;
        _mediator = mediator;
    }

    public async Task<bool> Handle(CancelContractPOCommand request, CancellationToken cancellationToken)
    {
        var result = await _commandRepository.CancelAsync(request.Id, cancellationToken);

        if (!result)
            throw new ExceptionRules("Contract PO not found.");

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Cancel",
            actionCode: "CONTRACT_PO_CANCEL",
            actionName: request.Id.ToString(),
            details: $"Contract PO with Id {request.Id} cancelled successfully.",
            module: "ContractPO");
        await _mediator.Publish(auditEvent, cancellationToken);

        return true;
    }
}
