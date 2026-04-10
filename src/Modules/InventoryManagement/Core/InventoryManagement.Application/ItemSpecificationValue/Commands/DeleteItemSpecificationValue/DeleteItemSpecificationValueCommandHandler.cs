
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Commands.DeleteItemSpecificationValue
{
    public class DeleteItemSpecificationValueCommandHandler : IRequestHandler<DeleteItemSpecificationValueCommand, bool>
    {
        private readonly IItemSpecificationValueCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteItemSpecificationValueCommandHandler(
            IItemSpecificationValueCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteItemSpecificationValueCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "ITEMSPECIFICATIONVALUE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Item Specification Value with Id {request.Id} soft deleted.",
                module: "ItemSpecificationValue"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
