using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.Journal.Commands.DeleteJournal
{
    public class DeleteJournalCommandHandler : IRequestHandler<DeleteJournalCommand, bool>
    {
        private readonly IJournalCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteJournalCommandHandler(
            IJournalCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteJournalCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "JOURNAL_DELETE",
                actionName: request.Id.ToString(),
                details: $"Journal voucher draft with Id {request.Id} soft deleted.",
                module: "Journal"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
