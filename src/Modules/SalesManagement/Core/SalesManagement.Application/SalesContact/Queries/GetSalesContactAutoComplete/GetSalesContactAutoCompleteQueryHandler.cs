using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesContact.Queries.GetSalesContactAutoComplete
{
    public class GetSalesContactAutoCompleteQueryHandler : IRequestHandler<GetSalesContactAutoCompleteQuery, IReadOnlyList<SalesContactLookupDto>>
    {
        private readonly ISalesContactQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesContactAutoCompleteQueryHandler(
            ISalesContactQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesContactLookupDto>> Handle(GetSalesContactAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<SalesContactLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesContactAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "Sales Contact details was fetched.",
                module: "SalesContact"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
