using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IArrival;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.Arrival.Commands.DeleteArrival
{
    public class DeleteArrivalCommandHandler : IRequestHandler<DeleteArrivalCommand, bool>
    {
        private readonly IArrivalCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteArrivalCommandHandler(
            IArrivalCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteArrivalCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!deleted)
                throw new ExceptionRules("Arrival not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "ARRIVAL_DELETE",
                actionName: request.Id.ToString(),
                details: $"Arrival with Id {request.Id} deleted successfully.",
                module: "Arrival");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
