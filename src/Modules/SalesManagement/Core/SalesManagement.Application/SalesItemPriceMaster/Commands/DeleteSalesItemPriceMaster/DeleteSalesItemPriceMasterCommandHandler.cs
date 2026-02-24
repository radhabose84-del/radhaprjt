#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesItemPriceMaster.Commands.DeleteSalesItemPriceMaster
{
    public sealed class DeleteSalesItemPriceMasterCommandHandler
        : IRequestHandler<DeleteSalesItemPriceMasterCommand, bool>
    {
        private readonly ISalesItemPriceMasterCommandRepository _commandRepo;
        private readonly ISalesItemPriceMasterQueryRepository _queryRepo;
        private readonly IMediator _mediator;

        public DeleteSalesItemPriceMasterCommandHandler(
            ISalesItemPriceMasterCommandRepository commandRepo,
            ISalesItemPriceMasterQueryRepository queryRepo,
            IMediator mediator)
        {
            _commandRepo = commandRepo;
            _queryRepo = queryRepo;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesItemPriceMasterCommand request, CancellationToken ct)
        {
            var before = await _queryRepo.GetByIdAsync(request.Id)
                         ?? throw new ExceptionRules("Sales Item Price Master not found.");

            var ok = await _commandRepo.SoftDeleteAsync(request.Id, ct);
            if (!ok) throw new ExceptionRules("Failed to delete Sales Item Price Master.");

            var ev = new AuditLogsDomainEvent(
                actionDetail: "SoftDelete",
                actionCode: before.PriceCode,
                actionName: before.PriceCode,
                details: $"Sales Item Price Master '{before.PriceCode}' soft-deleted.",
                module: "SalesItemPriceMaster"
            );
            await _mediator.Publish(ev, ct);

            return true;
        }
    }
}
