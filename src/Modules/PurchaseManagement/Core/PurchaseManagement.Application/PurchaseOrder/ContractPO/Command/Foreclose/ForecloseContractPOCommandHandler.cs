using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ContractPO.Command.Foreclose;

public class ForecloseContractPOCommandHandler : IRequestHandler<ForecloseContractPOCommand, bool>
{
    private readonly IContractPOCommandRepository _commandRepository;
    private readonly IMediator _mediator;

    public ForecloseContractPOCommandHandler(
        IContractPOCommandRepository commandRepository,
        IMediator mediator)
    {
        _commandRepository = commandRepository;
        _mediator = mediator;
    }

    public async Task<bool> Handle(ForecloseContractPOCommand request, CancellationToken cancellationToken)
    {
        var result = await _commandRepository.ForecloseAsync(request.Id, cancellationToken);

        if (!result)
            throw new ExceptionRules("Contract PO not found.");

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Foreclose",
            actionCode: "CONTRACT_PO_FORECLOSE",
            actionName: request.Id.ToString(),
            details: $"Contract PO with Id {request.Id} foreclosed successfully.",
            module: "ContractPO");
        await _mediator.Publish(auditEvent, cancellationToken);

        return true;
    }
}
