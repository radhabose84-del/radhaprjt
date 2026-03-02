using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesEnquiry.Queries.GetSalesEnquiryById
{
    public class GetSalesEnquiryByIdQueryHandler : IRequestHandler<GetSalesEnquiryByIdQuery, SalesEnquiryHeaderDto?>
    {
        private readonly ISalesEnquiryQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesEnquiryByIdQueryHandler(
            ISalesEnquiryQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<SalesEnquiryHeaderDto?> Handle(GetSalesEnquiryByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesEnquiryByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Sales Enquiry details {result.Id} was fetched.",
                module: "SalesEnquiry");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
