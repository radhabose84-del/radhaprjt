using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.Repacking.Commands.DeleteRepacking
{
    public class DeleteRepackingCommandHandler : IRequestHandler<DeleteRepackingCommand, bool>
    {
        private readonly IRepackingCommandRepository _commandRepository;
        private readonly IRepackingQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public DeleteRepackingCommandHandler(
            IRepackingCommandRepository commandRepository,
            IRepackingQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteRepackingCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Repacking not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "REPACKING_DELETE",
                actionName: request.Id.ToString(),
                details: $"Repacking with Id {request.Id} deleted successfully.",
                module: "Production"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
