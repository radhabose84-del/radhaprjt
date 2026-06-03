using MediatR;
using PurchaseManagement.Application.BarcodeSeries.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeSeries.Queries.GetBarcodeSeriesAutoComplete
{
    public class GetBarcodeSeriesAutoCompleteQueryHandler : IRequestHandler<GetBarcodeSeriesAutoCompleteQuery, IReadOnlyList<BarcodeSeriesLookupDto>>
    {
        private readonly IBarcodeSeriesQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetBarcodeSeriesAutoCompleteQueryHandler(IBarcodeSeriesQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<BarcodeSeriesLookupDto>> Handle(GetBarcodeSeriesAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetBarcodeSeriesAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Barcode series details was fetched.",
                module: "BarcodeSeries"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
