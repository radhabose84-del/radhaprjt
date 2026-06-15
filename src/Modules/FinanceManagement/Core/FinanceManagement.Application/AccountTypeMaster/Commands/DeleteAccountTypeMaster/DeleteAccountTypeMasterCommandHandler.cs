using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountTypeMaster.Commands.DeleteAccountTypeMaster
{
    public class DeleteAccountTypeMasterCommandHandler : IRequestHandler<DeleteAccountTypeMasterCommand, bool>
    {
        private readonly IAccountTypeMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteAccountTypeMasterCommandHandler(
            IAccountTypeMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteAccountTypeMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "ACCOUNT_TYPE_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Account Type Master with Id {request.Id} soft deleted.",
                module: "AccountTypeMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
