#nullable disable
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesItemPriceMaster.Commands.DeleteSalesItemPriceMaster
{
    public sealed class DeleteSalesItemPriceMasterCommandHandler
        : IRequestHandler<DeleteSalesItemPriceMasterCommand, bool>
    {
        private readonly ISalesItemPriceMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public DeleteSalesItemPriceMasterCommandHandler(
            ISalesItemPriceMasterCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesItemPriceMasterCommand request, CancellationToken cancellationToken)
        {
            await _commandRepository.SoftDeleteAsync(request.Id, cancellationToken);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: "SALES_ITEM_PRICE_DELETE",
                actionName: request.Id.ToString(),
                details: $"Sales Item Price Master with Id {request.Id} soft deleted.",
                module: "SalesItemPriceMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return true;
        }
    }
}
