#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeAutoComplete
{
    public class GetSalesOfficeAutoCompleteQueryHandler : IRequestHandler<GetSalesOfficeAutoCompleteQuery, IReadOnlyList<SalesOfficeLookupDto>>
    {
        private readonly ISalesOfficeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesOfficeAutoCompleteQueryHandler(ISalesOfficeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesOfficeLookupDto>> Handle(GetSalesOfficeAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var salesOffices = _mapper.Map<List<SalesOfficeLookupDto>>(result);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesOfficeAutoCompleteQuery",
                actionName: salesOffices.Count.ToString(),
                details: "SalesOffice details was fetched.",
                module: "SalesOffice"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesOffices;
        }
    }
}
