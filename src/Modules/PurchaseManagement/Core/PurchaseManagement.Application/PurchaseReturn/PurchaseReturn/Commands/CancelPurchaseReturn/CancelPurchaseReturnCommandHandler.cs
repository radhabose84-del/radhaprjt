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

        // Only a Pending Purchase Return can be cancelled (no Draft state; shared ApprovalStatus)
        if (!string.Equals(currentStatus, MiscEnumEntity.Pending, StringComparison.OrdinalIgnoreCase))
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
