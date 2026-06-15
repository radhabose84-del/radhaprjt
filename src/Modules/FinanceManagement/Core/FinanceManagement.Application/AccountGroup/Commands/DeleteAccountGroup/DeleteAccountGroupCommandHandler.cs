using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Commands.DeleteAccountGroup
{
    public class DeleteAccountGroupCommandHandler : IRequestHandler<DeleteAccountGroupCommand, bool>
    {
        private readonly IAccountGroupCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteAccountGroupCommandHandler(
            IAccountGroupCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteAccountGroupCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "ACCOUNT_GROUP_DELETE",
                actionName: request.Id.ToString(),
                details: $"Account Group with Id {request.Id} soft deleted.",
                module: "AccountGroup"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
