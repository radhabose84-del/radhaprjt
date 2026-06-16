using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.GlAccountMaster.Commands.DeleteGlAccountMaster
{
    public class DeleteGlAccountMasterCommandHandler : IRequestHandler<DeleteGlAccountMasterCommand, bool>
    {
        private readonly IGlAccountMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteGlAccountMasterCommandHandler(
            IGlAccountMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteGlAccountMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "GL_ACCOUNT_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"GL Account with Id {request.Id} soft deleted.",
                module: "GlAccountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
