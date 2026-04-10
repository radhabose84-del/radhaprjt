
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Commands.DeleteItemSpecificationMaster
{
    public class DeleteItemSpecificationMasterCommandHandler : IRequestHandler<DeleteItemSpecificationMasterCommand, bool>
    {
        private readonly IItemSpecificationMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteItemSpecificationMasterCommandHandler(
            IItemSpecificationMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteItemSpecificationMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "ITEMSPECIFICATIONMASTER_DELETE",
                actionName: request.Id.ToString(),
                details: $"Item Specification Master with Id {request.Id} soft deleted.",
                module: "ItemSpecificationMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
