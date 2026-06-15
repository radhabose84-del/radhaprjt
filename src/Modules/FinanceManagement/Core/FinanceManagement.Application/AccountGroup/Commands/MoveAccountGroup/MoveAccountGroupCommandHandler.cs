using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.MoveAccountGroup
{
    public class MoveAccountGroupCommandHandler : IRequestHandler<MoveAccountGroupCommand, ApiResponseDTO<int>>
    {
        private readonly IAccountGroupCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public MoveAccountGroupCommandHandler(
            IAccountGroupCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(MoveAccountGroupCommand request, CancellationToken cancellationToken)
        {
            // Circular-reference, level and single-parent rules are enforced in
            // MoveAccountGroupCommandValidator before this handler runs.
            // TODO (FR/approval): route this through the Finance Controller approval engine
            // (sp_EvaluateApproval) so the re-parent is applied only after approval. The
            // captured Justification + ApproverId below are the inputs that step will consume.
            var result = await _commandRepository.MoveAsync(request.Id, request.NewParentAccountGroupId);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Move",
                actionCode: "ACCOUNT_GROUP_MOVE",
                actionName: request.Id.ToString(),
                details: $"Account Group {request.Id} moved under parent {request.NewParentAccountGroupId}. " +
                         $"Approver: {request.ApproverId}. Justification: {request.Justification}",
                module: "AccountGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Account Group move processed successfully.",
                Data = result
            };
        }
    }
}
