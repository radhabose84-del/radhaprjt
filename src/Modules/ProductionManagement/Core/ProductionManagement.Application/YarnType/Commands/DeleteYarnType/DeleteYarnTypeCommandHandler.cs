using MediatR;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.YarnType.Commands.DeleteYarnType
{
    public class DeleteYarnTypeCommandHandler : IRequestHandler<DeleteYarnTypeCommand, bool>
    {
        private readonly IYarnTypeCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteYarnTypeCommandHandler(
            IYarnTypeCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteYarnTypeCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "YARNTYPE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Yarn Type with Id {request.Id} soft deleted.",
                module: "YarnType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
