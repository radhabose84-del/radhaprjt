using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Application.SalesQuotation.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Queries.GetSalesQuotationById
{
    public class GetSalesQuotationByIdQueryHandler : IRequestHandler<GetSalesQuotationByIdQuery, SalesQuotationHeaderDto?>
    {
        private readonly ISalesQuotationQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesQuotationByIdQueryHandler(
            ISalesQuotationQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<SalesQuotationHeaderDto?> Handle(GetSalesQuotationByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesQuotationByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Sales Quotation details {result.Id} was fetched.",
                module: "SalesQuotation");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
