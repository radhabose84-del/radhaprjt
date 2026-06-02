using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.ProcessApprovalDecision;

public sealed class ProcessPurchaseReturnApprovalDecisionCommandHandler
    : IRequestHandler<ProcessPurchaseReturnApprovalDecisionCommand, bool>
{
    private readonly IPurchaseReturnCommandRepository _commandRepo;
    private readonly IPurchaseReturnQueryRepository _queryRepo;
    private readonly IMediator _mediator;

    public ProcessPurchaseReturnApprovalDecisionCommandHandler(
        IPurchaseReturnCommandRepository commandRepo,
        IPurchaseReturnQueryRepository queryRepo,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _mediator = mediator;
    }

    public async Task<bool> Handle(ProcessPurchaseReturnApprovalDecisionCommand request, CancellationToken ct)
    {
        var currentStatus = await _queryRepo.GetCurrentStatusCodeAsync(request.PurchaseReturnHeaderId);
        if (currentStatus == null)
            throw new ExceptionRules("Purchase Return not found.");

        // Idempotency: ignore if not currently Pending (shared ApprovalStatus)
        if (!string.Equals(currentStatus, MiscEnumEntity.Pending, StringComparison.OrdinalIgnoreCase))
            return false;

        var targetCode = request.IsApproved ? MiscEnumEntity.Approved : MiscEnumEntity.Rejected;
        var targetStatusId = await _queryRepo.GetStatusIdByCodeAsync(targetCode)
            ?? throw new ExceptionRules($"RtvStatus '{targetCode}' not found in MiscMaster.");

        await _commandRepo.SetStatusAsync(request.PurchaseReturnHeaderId, targetStatusId, ct);

        // On Approved: write stock ledger (issue qty from receiving warehouse per detail line)
        if (request.IsApproved)
        {
            await _commandRepo.WriteStockLedgerOnApprovalAsync(request.PurchaseReturnHeaderId, ct);
        }

        var ev = new AuditLogsDomainEvent(
            actionDetail: request.IsApproved ? "Approved" : "Rejected",
            actionCode: request.IsApproved ? "PURCHASERETURN_APPROVED" : "PURCHASERETURN_REJECTED",
            actionName: request.PurchaseReturnHeaderId.ToString(),
            details: request.IsApproved
                ? $"Purchase Return {request.PurchaseReturnHeaderId} approved; stock ledger updated."
                : $"Purchase Return {request.PurchaseReturnHeaderId} rejected; no ledger movement.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return true;
    }
}
