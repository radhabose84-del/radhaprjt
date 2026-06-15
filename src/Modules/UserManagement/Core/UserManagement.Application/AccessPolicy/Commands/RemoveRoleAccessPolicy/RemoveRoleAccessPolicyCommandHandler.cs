using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Domain.Events;

namespace UserManagement.Application.AccessPolicy.Commands.RemoveRoleAccessPolicy
{
    public class RemoveRoleAccessPolicyCommandHandler : IRequestHandler<RemoveRoleAccessPolicyCommand, bool>
    {
        private readonly IAccessPolicyCommandRepository _commandRepository;
        private readonly IMediator                      _mediator;

        public RemoveRoleAccessPolicyCommandHandler(
            IAccessPolicyCommandRepository commandRepository,
            IMediator                      mediator)
        {
            _commandRepository = commandRepository;
            _mediator          = mediator;
        }

        public async Task<bool> Handle(
            RemoveRoleAccessPolicyCommand request, CancellationToken cancellationToken)
        {
            var removed = await _commandRepository.RemoveRoleValueAsync(request.Id, cancellationToken);

            if (!removed)
                throw new ExceptionRules("Role access policy assignment not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode:   "ROLEACCESSPOLICY_REMOVE",
                actionName:   request.Id.ToString(),
                details:      $"RoleAccessPolicy with Id {request.Id} was removed.",
                module:       "AccessPolicy"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
