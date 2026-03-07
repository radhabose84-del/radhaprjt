using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.DeliveryChallan.Commands.DeleteDeliveryChallan
{
    public class DeleteDeliveryChallanCommandHandler : IRequestHandler<DeleteDeliveryChallanCommand, bool>
    {
        private readonly IDeliveryChallanCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteDeliveryChallanCommandHandler(
            IDeliveryChallanCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteDeliveryChallanCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!result)
                throw new ExceptionRules("Delivery Challan not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "DELIVERYCHALLAN_DELETE",
                actionName: request.Id.ToString(),
                details: $"Delivery Challan with Id {request.Id} soft deleted.",
                module: "DeliveryChallan");
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
