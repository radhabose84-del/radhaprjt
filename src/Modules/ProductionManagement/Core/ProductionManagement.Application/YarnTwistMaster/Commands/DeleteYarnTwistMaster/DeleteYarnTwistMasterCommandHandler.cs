using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnTwistMaster.Commands.DeleteYarnTwistMaster
{
    public class DeleteYarnTwistMasterCommandHandler : IRequestHandler<DeleteYarnTwistMasterCommand, bool>
    {
        private readonly IYarnTwistMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteYarnTwistMasterCommandHandler(
            IYarnTwistMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteYarnTwistMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "YARNTWISTMASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Yarn Twist Master with Id {request.Id} soft deleted.",
                module: "YarnTwistMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
