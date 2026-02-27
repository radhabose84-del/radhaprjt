using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesLead.Queries.GetSalesLeadAutoComplete
{
    public class GetSalesLeadAutoCompleteQueryHandler : IRequestHandler<GetSalesLeadAutoCompleteQuery, IReadOnlyList<SalesLeadLookupDto>>
    {
        private readonly ISalesLeadQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesLeadAutoCompleteQueryHandler(ISalesLeadQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesLeadLookupDto>> Handle(GetSalesLeadAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);

            var dtos = _mapper.Map<List<SalesLeadLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesLeadAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "SalesLead details was fetched.",
                module: "SalesLead"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
