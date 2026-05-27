using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Events;
using System.Text.Json;

namespace PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.SubmitPurchaseReturn;

public sealed class SubmitPurchaseReturnCommandHandler : IRequestHandler<SubmitPurchaseReturnCommand, bool>
{
    private readonly IPurchaseReturnCommandRepository _commandRepo;
    private readonly IPurchaseReturnQueryRepository _queryRepo;
    private readonly IOutboxEventPublisher _outboxEventPublisher;
    private readonly IIPAddressService _ip;
    private readonly IMediator _mediator;

    public SubmitPurchaseReturnCommandHandler(
        IPurchaseReturnCommandRepository commandRepo,
        IPurchaseReturnQueryRepository queryRepo,
        IOutboxEventPublisher outboxEventPublisher,
        IIPAddressService ip,
        IMediator mediator)
    {
        _commandRepo = commandRepo;
        _queryRepo = queryRepo;
        _outboxEventPublisher = outboxEventPublisher;
        _ip = ip;
        _mediator = mediator;
    }

    public async Task<bool> Handle(SubmitPurchaseReturnCommand request, CancellationToken ct)
    {
        var header = await _queryRepo.GetByIdAsync(request.Id, ct)
            ?? throw new ExceptionRules("Purchase Return not found.");

        if (!string.Equals(header.StatusCode, MiscEnumEntity.Draft, StringComparison.OrdinalIgnoreCase))
            throw new ExceptionRules("Only Draft Purchase Returns can be submitted.");

        // Resolve PendingApproval status id
        var pendingStatusId = await _queryRepo.GetStatusIdByCodeAsync(MiscEnumEntity.RtvPendingApproval)
            ?? throw new ExceptionRules("RtvStatus 'PendingApproval' not found in MiscMaster.");

        await _commandRepo.SetStatusAsync(request.Id, pendingStatusId, ct);

        // Resolve approval routing key from ReturnType.ApprovalRoleCode
        var approvalRoleCode = await _queryRepo.GetReturnTypeApprovalRoleCodeAsync(header.ReturnTypeId);
        var routeKey = string.IsNullOrWhiteSpace(approvalRoleCode)
            ? $"{MiscEnumEntity.RtvModuleTypeName}:Default"
            : $"{MiscEnumEntity.RtvModuleTypeName}:{approvalRoleCode}";

        // Publish approval request via Outbox
        var correlationId = Guid.NewGuid();
        var payload = JsonSerializer.Serialize(new
        {
            Header = new
            {
                Id = request.Id,
                RtvNumber = header.RtvNumber,
                VendorId = header.VendorId,
                UnitId = header.UnitId,
                ReturnTypeId = header.ReturnTypeId,
                RouteKey = routeKey
            }
        });

        var workflowEvent = new CreateApprovalRequestCommand
        {
            CorrelationId = correlationId,
            ModuleTypeName = MiscEnumEntity.RtvModuleTypeName,
            ModuleTransactionId = request.Id,
            Payload = payload
        };
        await _outboxEventPublisher.ScheduleAsync(workflowEvent, correlationId, ct);

        var ev = new AuditLogsDomainEvent(
            actionDetail: "Submit",
            actionCode: "PURCHASERETURN_SUBMIT",
            actionName: header.RtvNumber,
            details: $"Purchase Return '{header.RtvNumber}' submitted for approval; routeKey={routeKey}.",
            module: "PurchaseReturn");
        await _mediator.Publish(ev, ct);

        return true;
    }
}
