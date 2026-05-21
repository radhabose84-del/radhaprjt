using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.Foreclose;

public class ForecloseImportPOCommandHandler : IRequestHandler<ForecloseImportPOCommand, bool>
{
    private readonly IImportPOCommandRepository _commandRepository;
    private readonly IMediator _mediator;

    public ForecloseImportPOCommandHandler(
        IImportPOCommandRepository commandRepository,
        IMediator mediator)
    {
        _commandRepository = commandRepository;
        _mediator = mediator;
    }

    public async Task<bool> Handle(ForecloseImportPOCommand request, CancellationToken cancellationToken)
    {
        var result = await _commandRepository.ForecloseAsync(request.Id, cancellationToken);

        if (!result)
            throw new ExceptionRules("Import PO not found.");

        var auditEvent = new AuditLogsDomainEvent(
            actionDetail: "Foreclose",
            actionCode: "IMPORT_PO_FORECLOSE",
            actionName: request.Id.ToString(),
            details: $"Import PO with Id {request.Id} foreclosed successfully.",
            module: "ImportPO");
        await _mediator.Publish(auditEvent, cancellationToken);

        return true;
    }
}
