using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CancelPurchaseReturn;

public sealed class CancelPurchaseReturnCommandHandler : IRequestHandler<CancelPurchaseReturnCommand, bool>
{
    private readonly IPurchaseReturnCommandRepository _commandRepo;
    private readonly IPurchaseReturnQueryRepository _queryRepo;
    private readonly IMediator _mediator;

    public CancelPurchaseReturnCommandHandler(
        IPurchaseReturnCommandRepository commandRepo,
        IPurchaseReturnQueryRepository queryRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(CancelPurchaseReturnCommand request, CancellationToken ct)
    {
        var currentStatus = await _queryRepo.GetCurrentStatusCodeAsync(request.Id)
            ?? throw new ExceptionRules("Purchase Return not found.");

        // Only Draft or PendingApproval can be cancelled
        var isDraft = string.Equals(currentStatus, MiscEnumEntity.Draft, StringComparison.OrdinalIgnoreCase);
        var isPending = string.Equals(currentStatus, MiscEnumEntity.RtvPendingApproval, StringComparison.OrdinalIgnoreCase);
        if (!isDraft && !isPending)
            throw new ExceptionRules($"Cannot cancel Purchase Return in status '{currentStatus}'.");

        var cancelledStatusId = await _queryRepo.GetStatusIdByCodeAsync(MiscEnumEntity.Cancelled)
            ?? throw new ExceptionRules("RtvStatus 'Cancelled' not found in MiscMaster.");

        await _commandRepo.SetStatusAsync(request.Id, cancelledStatusId, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Cancel",
            actionCode: "PURCHASERETURN_CANCEL",
            actionName: request.Id.ToString(),
            details: $"Purchase Return with Id {request.Id} cancelled (was {currentStatus}).",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return true;
    }
}
