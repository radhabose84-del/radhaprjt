using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
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
        private readonly IMediator _mediator;

        public MoveAccountGroupCommandHandler(
            IAccountGroupChangeRequestRepository changeRequestRepository,
            IOutboxEventPublisher outboxEventPublisher,
            IMediator mediator)
        {
            _changeRequestRepository = changeRequestRepository;
            _outboxEventPublisher = outboxEventPublisher;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(MoveAccountGroupCommand request, CancellationToken cancellationToken)
        {
            // Circular-reference, level, justification and approver are validated in
            // MoveAccountGroupCommandValidator before this handler runs. The move is NOT applied
            // now — it is deferred until the Finance Controller approves. We persist a pending
            // change request and raise the approval request atomically (same SaveChanges).
            var changeRequest = new Domain.Entities.AccountGroupChangeRequest
            {
                AccountGroupId = request.Id,
                NewParentAccountGroupId = request.NewParentAccountGroupId,
                Justification = request.Justification,
                ApproverId = request.ApproverId,
                RequestStatus = MiscEnumEntity.Pending
            };
            await _changeRequestRepository.AddWithoutSaveAsync(changeRequest, cancellationToken);

            var correlationId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                request.Id,
                request.NewParentAccountGroupId,
                request.Justification,
                request.ApproverId
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
                         $"submitted for approval. Approver: {request.ApproverId}. Justification: {request.Justification}",
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
