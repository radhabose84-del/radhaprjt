using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.Application.AccessPolicy.Commands.DeleteAccessPolicy
{
    public class DeleteAccessPolicyCommandHandler : IRequestHandler<DeleteAccessPolicyCommand, bool>
    {
        private readonly IAccessPolicyCommandRepository _commandRepository;
        private readonly IMediator                      _mediator;

        public DeleteAccessPolicyCommandHandler(
            IAccessPolicyCommandRepository commandRepository,
            IMediator                      mediator)
        {
            _commandRepository = commandRepository;
            _mediator          = mediator;
        }

        public async Task<bool> Handle(
            DeleteAccessPolicyCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!deleted)
                throw new ExceptionRules("Access Policy not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode:   "ACCESSPOLICY_DELETE",
                actionName:   request.Id.ToString(),
                details:      $"AccessPolicy with Id {request.Id} was soft-deleted.",
                module:       "AccessPolicy"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
