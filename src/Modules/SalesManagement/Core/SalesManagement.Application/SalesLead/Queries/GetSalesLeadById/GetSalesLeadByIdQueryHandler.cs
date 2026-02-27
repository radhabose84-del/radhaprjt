using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesLead.Queries.GetSalesLeadById
{
    public class GetSalesLeadByIdQueryHandler : IRequestHandler<GetSalesLeadByIdQuery, SalesLeadDto?>
    {
        private readonly ISalesLeadQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesLeadByIdQueryHandler(ISalesLeadQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<SalesLeadDto?> Handle(GetSalesLeadByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<SalesLeadDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesLeadByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"SalesLead details {dto.Id} was fetched.",
                module: "SalesLead"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
