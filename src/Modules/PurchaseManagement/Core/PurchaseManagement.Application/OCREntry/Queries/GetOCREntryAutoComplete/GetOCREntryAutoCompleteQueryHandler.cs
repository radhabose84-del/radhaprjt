using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCREntryAutoComplete
{
    public class GetOCREntryAutoCompleteQueryHandler : IRequestHandler<GetOCREntryAutoCompleteQuery, IReadOnlyList<OCREntryLookupDto>>
    {
        private readonly IOCREntryQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetOCREntryAutoCompleteQueryHandler(IOCREntryQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<OCREntryLookupDto>> Handle(GetOCREntryAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken, request.Approved, request.ShowAll);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetOCREntryAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "OCR details was fetched.",
                module: "OCREntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
