using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.DeleteJournalThresholdRule
{
    public class DeleteJournalThresholdRuleCommandHandler : IRequestHandler<DeleteJournalThresholdRuleCommand, bool>
    {
        private readonly IJournalThresholdRuleCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteJournalThresholdRuleCommandHandler(
            IJournalThresholdRuleCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteJournalThresholdRuleCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "JOURNAL_THRESHOLD_DELETE",
                actionName: request.Id.ToString(),
                details: $"Journal threshold rule with Id {request.Id} soft deleted.",
                module: "JournalThresholdRule"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
