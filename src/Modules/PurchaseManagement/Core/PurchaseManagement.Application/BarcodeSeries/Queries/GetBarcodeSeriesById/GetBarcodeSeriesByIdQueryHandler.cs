using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesById
{
    public class GetBarcodeSeriesByIdQueryHandler : IRequestHandler<GetBarcodeSeriesByIdQuery, BarcodeSeriesDto?>
    {
        private readonly IBarcodeSeriesQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetBarcodeSeriesByIdQueryHandler(IBarcodeSeriesQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<BarcodeSeriesDto?> Handle(GetBarcodeSeriesByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetBarcodeSeriesByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Barcode series details {result.Id} was fetched.",
                module: "BarcodeSeries"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
