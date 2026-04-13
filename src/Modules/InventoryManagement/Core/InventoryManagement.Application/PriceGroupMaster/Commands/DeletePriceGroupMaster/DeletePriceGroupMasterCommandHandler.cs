using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Commands.DeletePriceGroupMaster
{
    public class DeletePriceGroupMasterCommandHandler : IRequestHandler<DeletePriceGroupMasterCommand, bool>
    {
        private readonly IPriceGroupMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeletePriceGroupMasterCommandHandler(
            IPriceGroupMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeletePriceGroupMasterCommand request, CancellationToken cancellationToken)
        {
            var deleted = await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            if (!deleted)
                throw new ExceptionRules($"Price Group with Id {request.Id} not found.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "PRICEGROUP_DELETE",
                actionName: request.Id.ToString(),
                details: $"Price Group with Id {request.Id} was soft deleted.",
                module: "PriceGroupMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
