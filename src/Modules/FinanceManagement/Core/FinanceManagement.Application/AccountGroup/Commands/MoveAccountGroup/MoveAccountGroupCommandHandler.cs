using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.MoveAccountGroup
{
    public class MoveAccountGroupCommandHandler : IRequestHandler<MoveAccountGroupCommand, ApiResponseDTO<int>>
    {
        private readonly IAccountGroupChangeRequestRepository _changeRequestRepository;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public MoveAccountGroupCommandHandler(
            IAccountGroupChangeRequestRepository changeRequestRepository,
            IOutboxEventPublisher outboxEventPublisher,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _changeRequestRepository = changeRequestRepository;
            _outboxEventPublisher = outboxEventPublisher;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(MoveAccountGroupCommand request, CancellationToken cancellationToken)
        {
            // Circular-reference, level and justification are validated in
            // MoveAccountGroupCommandValidator before this handler runs. The move is NOT applied
            // now — it is deferred until the multilevel approval chain (Finance Controller → CFO)
            // completes. We persist a pending change request and raise the approval request
            // atomically (same SaveChanges). Approvers come from the workflow config, not this command.
            var changeRequest = new Domain.Entities.AccountGroupChangeRequest
            {
                AccountGroupId = request.Id,
                NewParentAccountGroupId = request.NewParentAccountGroupId,
                Justification = request.Justification,
                RequestStatus = MiscEnumEntity.Pending
            };
            await _changeRequestRepository.AddWithoutSaveAsync(changeRequest, cancellationToken);

            var correlationId = Guid.NewGuid();

            // The approval engine is unit-scoped: ApprovalRequest.UnitId is NOT NULL and is taken
            // from the transaction payload. Stamp the acting user's operating unit (same as PO/Sales).
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            // sp_EvaluateApproval reads the transaction unit from $.Header.UnitId (same shape the
            // PO/Sales workflows use), so the payload is wrapped in a Header object.
            var payload = JsonSerializer.Serialize(new
            {
                Header = new
                {
                    request.Id,
                    request.NewParentAccountGroupId,
                    request.Justification,
                    UnitId = unitId
                }
            });

            // ModuleTypeName MUST equal the Menu name (MenuId 1288) so sp_EvaluateApproval resolves it.
            var approvalRequest = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.AccountGroupHierarchy,
                ModuleTransactionId = request.Id,
                Payload = payload,
                TransactionTypeId = null
            };
            await _outboxEventPublisher.ScheduleWithoutSaveAsync(approvalRequest, correlationId, cancellationToken);

            // Commits the pending request + the outbox message in one transaction.
            await _changeRequestRepository.SaveChangesAsync(cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Move",
                actionCode: "ACCOUNT_GROUP_MOVE_REQUESTED",
                actionName: request.Id.ToString(),
                details: $"Account Group {request.Id} move under parent {request.NewParentAccountGroupId} " +
                         $"submitted for multilevel approval. Justification: {request.Justification}",
                module: "AccountGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Account Group move submitted for Finance Controller approval.",
                Data = request.Id
            };
        }
    }
}
