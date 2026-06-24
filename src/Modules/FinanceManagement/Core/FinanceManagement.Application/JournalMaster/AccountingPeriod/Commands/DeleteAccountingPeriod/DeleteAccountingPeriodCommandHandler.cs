using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.DeleteAccountingPeriod
{
    public class DeleteAccountingPeriodCommandHandler : IRequestHandler<DeleteAccountingPeriodCommand, bool>
    {
        private readonly IAccountingPeriodCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteAccountingPeriodCommandHandler(
            IAccountingPeriodCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteAccountingPeriodCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "ACCOUNTING_PERIOD_DELETE",
                actionName: request.Id.ToString(),
                details: $"Accounting Period with Id {request.Id} soft deleted.",
                module: "AccountingPeriod"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
