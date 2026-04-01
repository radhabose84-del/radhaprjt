using Contracts.Common;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingMaster.Commands.DeleteRepackingMaster
{
    public class DeleteRepackingMasterCommandHandler
        : IRequestHandler<DeleteRepackingMasterCommand, bool>
    {
        private readonly IRepackingMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteRepackingMasterCommandHandler(
            IRepackingMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(
            DeleteRepackingMasterCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Repacking not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "REPACKING_DELETE",
                actionName: request.Id.ToString(),
                details: $"Repacking with Id {request.Id} soft deleted successfully.",
                module: "RepackingMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
