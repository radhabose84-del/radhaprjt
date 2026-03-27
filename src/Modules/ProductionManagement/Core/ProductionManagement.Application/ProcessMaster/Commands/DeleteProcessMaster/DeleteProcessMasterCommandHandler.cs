using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProcessMaster.Commands.DeleteProcessMaster
{
    public class DeleteProcessMasterCommandHandler : IRequestHandler<DeleteProcessMasterCommand, bool>
    {
        private readonly IProcessMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteProcessMasterCommandHandler(
            IProcessMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteProcessMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "PROCESSMASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Process Master with Id {request.Id} soft deleted.",
                module: "ProcessMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
