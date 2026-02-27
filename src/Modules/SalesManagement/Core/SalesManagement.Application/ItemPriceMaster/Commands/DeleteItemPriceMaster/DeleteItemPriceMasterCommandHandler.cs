using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ItemPriceMaster.Commands.DeleteItemPriceMaster
{
    public sealed class DeleteItemPriceMasterCommandHandler
        : IRequestHandler<DeleteItemPriceMasterCommand, bool>
    {
        private readonly IItemPriceMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteItemPriceMasterCommandHandler(
            IItemPriceMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteItemPriceMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "ITEM_PRICE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Item Price Master with Id {request.Id} soft deleted.",
                module: "ItemPriceMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
