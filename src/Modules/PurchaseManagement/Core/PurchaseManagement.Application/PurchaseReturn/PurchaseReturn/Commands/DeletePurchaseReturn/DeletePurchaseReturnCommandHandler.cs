using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.DeletePurchaseReturn;

public sealed class DeletePurchaseReturnCommandHandler : IRequestHandler<DeletePurchaseReturnCommand, bool>
{
    private readonly IPurchaseReturnCommandRepository _commandRepo;
    private readonly IPurchaseReturnQueryRepository _queryRepo;
    private readonly IMediator _mediator;

    public DeletePurchaseReturnCommandHandler(
        IPurchaseReturnCommandRepository commandRepo,
        IPurchaseReturnQueryRepository queryRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(DeletePurchaseReturnCommand request, CancellationToken ct)
    {
        var currentStatus = await _queryRepo.GetCurrentStatusCodeAsync(request.Id)
            ?? throw new ExceptionRules("Purchase Return not found.");

        if (!string.Equals(currentStatus, MiscEnumEntity.Draft, StringComparison.OrdinalIgnoreCase))
            throw new ExceptionRules("Only Draft Purchase Returns can be deleted.");

        var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
        if (!ok)
            throw new ExceptionRules("Failed to soft-delete Purchase Return.");

        var ev = new AuditLogsDomainEvent(
            actionDetail: "SoftDelete",
            actionCode: "PURCHASERETURN_DELETE",
            actionName: request.Id.ToString(),
            details: $"Purchase Return with Id {request.Id} soft-deleted.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return true;
    }
}
