using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete
{
    public class GetItemAutoCompleteQueryHandler
        : IRequestHandler<GetItemAutoCompleteQuery, List<GetItemAutoCompleteDto>>
    {
        private readonly IItemQueryRepository _itemQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetItemAutoCompleteQueryHandler(
            IItemQueryRepository itemQueryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _itemQueryRepository = itemQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<List<GetItemAutoCompleteDto>> Handle(
            GetItemAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _itemQueryRepository.GetItemAutoCompleteAsync(request.SearchPattern ?? string.Empty,request.ItemGroupId, request.ItemCategoryId,request.SourceId,request.IssueRuleId, cancellationToken);
            var items = _mapper.Map<List<GetItemAutoCompleteDto>>(result);

            // Domain event (optional)
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAutocomplete",
                actionCode: nameof(GetItemAutoCompleteQueryHandler),
                actionName: items.Count.ToString(),
                details: "Item autocomplete fetched.",
                module: "ItemMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return items;
        }
    }
}
