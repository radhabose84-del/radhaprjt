using MediatR;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.CountMaster.Commands.DeleteCountMaster
{
    public class DeleteCountMasterCommandHandler : IRequestHandler<DeleteCountMasterCommand, bool>
    {
        private readonly ICountMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteCountMasterCommandHandler(
            ICountMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteCountMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "COUNT_MASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Count Master with Id {request.Id} soft deleted.",
                module: "CountMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
