using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProductionPack.Queries.GetLastStockLedgerDate
{
    public class GetLastStockLedgerDateQueryHandler : IRequestHandler<GetLastStockLedgerDateQuery, DateOnly?>
    {
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetLastStockLedgerDateQueryHandler(
            IProductionQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<DateOnly?> Handle(GetLastStockLedgerDateQuery request, CancellationToken cancellationToken)
        {
            var lastDate = await _queryRepository.GetLastStockLedgerDateAsync(request.DayClose);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetLastStockLedgerDate",
                actionCode: "GetLastStockLedgerDateQuery",
                actionName: lastDate?.ToString() ?? "None",
                details: "Last ProductionStockLedger date was fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return lastDate;
        }
    }
}
