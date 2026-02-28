using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitAutoComplete
{
    public class GetCustomerVisitAutoCompleteQueryHandler : IRequestHandler<GetCustomerVisitAutoCompleteQuery, IReadOnlyList<CustomerVisitLookupDto>>
    {
        private readonly ICustomerVisitQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCustomerVisitAutoCompleteQueryHandler(ICustomerVisitQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<CustomerVisitLookupDto>> Handle(GetCustomerVisitAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<CustomerVisitLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetCustomerVisitAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "CustomerVisit details was fetched.",
                module: "CustomerVisit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
