using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.OCREntry.Queries.GetOCREntryById
{
    public class GetOCREntryByIdQueryHandler : IRequestHandler<GetOCREntryByIdQuery, OCREntryDto?>
    {
        private readonly IOCREntryQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetOCREntryByIdQueryHandler(IOCREntryQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<OCREntryDto?> Handle(GetOCREntryByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetOCREntryByIdQuery",
                actionName: result.Id.ToString(),
                details: $"OCR details {result.Id} was fetched.",
                module: "OCREntry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
