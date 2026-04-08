using MediatR;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.ProductionPack.Queries.GetPreviousDateClosing
{
    public class GetPreviousDateClosingQueryHandler : IRequestHandler<GetPreviousDateClosingQuery, ProductionStockClosingDto?>
    {
        private readonly IProductionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetPreviousDateClosingQueryHandler(
            IProductionQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ProductionStockClosingDto?> Handle(GetPreviousDateClosingQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetPreviousDateClosingAsync(
                request.ItemId, request.LotId, request.DocDate);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPreviousDateClosing",
                actionCode: "GetPreviousDateClosingQuery",
                actionName: $"{request.ItemId}-{request.LotId}",
                details: $"Previous date closing for Item {request.ItemId}, Lot {request.LotId} before {request.DocDate} was fetched.",
                module: "Production"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
