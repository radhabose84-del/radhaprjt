using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterAutoComplete
{
    public class GetItemPriceMasterAutoCompleteQueryHandler
        : IRequestHandler<GetItemPriceMasterAutoCompleteQuery, IReadOnlyList<ItemPriceMasterLookupDto>>
    {
        private readonly IItemPriceMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetItemPriceMasterAutoCompleteQueryHandler(IItemPriceMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<ItemPriceMasterLookupDto>> Handle(
            GetItemPriceMasterAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken);
            var salesItemPriceMasters = _mapper.Map<List<ItemPriceMasterLookupDto>>(result);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetItemPriceMasterAutoCompleteQuery",
                actionName: salesItemPriceMasters.Count.ToString(),
                details: "ItemPriceMaster details was fetched.",
                module: "ItemPriceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesItemPriceMasters;
        }
    }
}
