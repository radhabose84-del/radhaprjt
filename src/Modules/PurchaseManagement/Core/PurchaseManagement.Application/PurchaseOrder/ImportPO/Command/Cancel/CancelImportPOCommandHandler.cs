using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Cancel;

public class CancelImportPOCommandHandler : IRequestHandler<CancelImportPOCommand, bool>
{
    private readonly IImportPOCommandRepository _commandRepository;
    private readonly IMediator _mediator;

    public CancelImportPOCommandHandler(
        IImportPOCommandRepository commandRepository,
        IMediator mediator)
    {
        _commandRepository = commandRepository;
        _mediator = mediator;
    }

    public async Task<bool> Handle(CancelImportPOCommand request, CancellationToken cancellationToken)
    {
        var result = await _commandRepository.CancelAsync(request.Id, cancellationToken);

        if (!result)
            throw new ExceptionRules("Import PO not found.");

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Cancel",
            actionCode: "IMPORT_PO_CANCEL",
            actionName: request.Id.ToString(),
            details: $"Import PO with Id {request.Id} cancelled successfully.",
            module: "ImportPO");
        await _mediator.Publish(auditEvent, cancellationToken);

        return true;
    }
}
