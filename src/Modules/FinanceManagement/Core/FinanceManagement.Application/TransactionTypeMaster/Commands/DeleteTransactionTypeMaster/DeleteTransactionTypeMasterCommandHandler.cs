using FinanceManagement.Application.Common.Interfaces.ITransactionTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TransactionTypeMaster.Commands.DeleteTransactionTypeMaster
{
    public class DeleteTransactionTypeMasterCommandHandler : IRequestHandler<DeleteTransactionTypeMasterCommand, bool>
    {
        private readonly ITransactionTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteTransactionTypeMasterCommandHandler(
            ITransactionTypeMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteTransactionTypeMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "TRANSACTION_TYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Transaction Type Master with Id {request.Id} soft deleted.",
                module: "TransactionTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
