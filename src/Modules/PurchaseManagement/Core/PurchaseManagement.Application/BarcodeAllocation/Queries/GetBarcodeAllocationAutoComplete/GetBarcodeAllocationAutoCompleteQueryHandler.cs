using MediatR;
using PurchaseManagement.Application.BarcodeAllocation.Dto;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.BarcodeAllocation.Queries.GetBarcodeAllocationAutoComplete
{
    public class GetBarcodeAllocationAutoCompleteQueryHandler : IRequestHandler<GetBarcodeAllocationAutoCompleteQuery, IReadOnlyList<BarcodeAllocationLookupDto>>
    {
        private readonly IBarcodeAllocationQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetBarcodeAllocationAutoCompleteQueryHandler(IBarcodeAllocationQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<BarcodeAllocationLookupDto>> Handle(GetBarcodeAllocationAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetBarcodeAllocationAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "Barcode allocation details was fetched.",
                module: "BarcodeAllocation"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
