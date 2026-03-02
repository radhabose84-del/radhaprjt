using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemsByVariantFilter
{
    public class GetItemsByVariantFilterQueryHandler
        : IRequestHandler<GetItemsByVariantFilterQuery, List<GetItemAutoCompleteDto>>
    {
        private readonly IItemQueryRepository _itemQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetItemsByVariantFilterQueryHandler(
            IItemQueryRepository itemQueryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _itemQueryRepository = itemQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<List<GetItemAutoCompleteDto>> Handle(
            GetItemsByVariantFilterQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _itemQueryRepository.GetItemsByVariantFilterAsync(
                request.HasVariant, request.ParentItemId, cancellationToken);

            var items = _mapper.Map<List<GetItemAutoCompleteDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetItemsByVariantFilter",
                actionCode: nameof(GetItemsByVariantFilterQueryHandler),
                actionName: items.Count.ToString(),
                details: "Items fetched by variant filter.",
                module: "ItemMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return items;
        }
    }
}
