using AutoMapper;
using MediatR;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.BusinessUnit.Queries.GetBusinessUnitAutoComplete
{
    public class GetBusinessUnitAutoCompleteQueryHandler : IRequestHandler<GetBusinessUnitAutoCompleteQuery, IReadOnlyList<BusinessUnitLookupDto>>
    {
        private readonly IBusinessUnitQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetBusinessUnitAutoCompleteQueryHandler(IBusinessUnitQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<BusinessUnitLookupDto>> Handle(GetBusinessUnitAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term, cancellationToken);
            var businessUnits = _mapper.Map<List<BusinessUnitLookupDto>>(result);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetBusinessUnitAutoCompleteQuery",
                actionName: businessUnits.Count.ToString(),
                details: "BusinessUnit details was fetched.",
                module: "BusinessUnit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return businessUnits;
        }
    }
}
