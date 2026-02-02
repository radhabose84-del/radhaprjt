using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationCompare;
using PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparisonById;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.PartyMaster.Queries.GetPartyMasterById
{
    public class GetQuoteComparisonByIdQueryHandler : IRequestHandler<GetQuoteComparisonByIdQuery, QuoteCompareByIdDto>
    {
        private readonly IQuotationCompareQueryRepository _iQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;      

        public GetQuoteComparisonByIdQueryHandler(IQuotationCompareQueryRepository iQueryRepository, IMapper mapper, IMediator mediator)
        {
            _iQueryRepository = iQueryRepository;
            _mapper = mapper;
            _mediator = mediator;      
        }
        public async Task<QuoteCompareByIdDto> Handle(GetQuoteComparisonByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _iQueryRepository.GetByIdQuoteCompareAsync(request.Id);             
            if (dto == null)
                throw new KeyNotFoundException("Quote Comparison not found");

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetQuoteComparisonByIdQueryHandler",
                actionName: dto.Id.ToString(),
                details: $"Quote Comparison details {dto.Id} fetched.",
                module: "Quote Comparison"
            );

            await _mediator.Publish(domainEvent, cancellationToken);
            return dto;
        }
    }
}