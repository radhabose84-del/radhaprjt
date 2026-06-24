using FinanceManagement.Application.Common.Interfaces.JournalMaster.IRecurringJournalTemplate;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.DeleteRecurringJournalTemplate
{
    public class DeleteRecurringJournalTemplateCommandHandler : IRequestHandler<DeleteRecurringJournalTemplateCommand, bool>
    {
        private readonly IRecurringJournalTemplateCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteRecurringJournalTemplateCommandHandler(
            IRecurringJournalTemplateCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteRecurringJournalTemplateCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "RECURRING_TEMPLATE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Recurring journal template with Id {request.Id} soft deleted.",
                module: "RecurringJournalTemplate"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
