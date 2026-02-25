#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterAutoComplete
{
    public class GetSalesItemPriceMasterAutoCompleteQueryHandler
        : IRequestHandler<GetSalesItemPriceMasterAutoCompleteQuery, IReadOnlyList<SalesItemPriceMasterLookupDto>>
    {
        private readonly ISalesItemPriceMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesItemPriceMasterAutoCompleteQueryHandler(ISalesItemPriceMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesItemPriceMasterLookupDto>> Handle(
            GetSalesItemPriceMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var salesItemPriceMasters = _mapper.Map<List<SalesItemPriceMasterLookupDto>>(result);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesItemPriceMasterAutoCompleteQuery",
                actionName: salesItemPriceMasters.Count.ToString(),
                details: "SalesItemPriceMaster details was fetched.",
                module: "SalesItemPriceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesItemPriceMasters;
        }
    }
}
