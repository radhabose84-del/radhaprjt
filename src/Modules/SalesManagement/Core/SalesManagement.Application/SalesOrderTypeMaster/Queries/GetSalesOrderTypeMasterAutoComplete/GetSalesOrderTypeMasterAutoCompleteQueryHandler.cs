using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrderTypeMaster.Queries.GetSalesOrderTypeMasterAutoComplete
{
    public class GetSalesOrderTypeMasterAutoCompleteQueryHandler
        : IRequestHandler<GetSalesOrderTypeMasterAutoCompleteQuery, IReadOnlyList<SalesOrderTypeMasterLookupDto>>
    {
        private readonly ISalesOrderTypeMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesOrderTypeMasterAutoCompleteQueryHandler(
            ISalesOrderTypeMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesOrderTypeMasterLookupDto>> Handle(
            GetSalesOrderTypeMasterAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var dtos = _mapper.Map<List<SalesOrderTypeMasterLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesOrderTypeMasterAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "SalesOrderTypeMaster autocomplete was fetched.",
                module: "SalesOrderTypeMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
