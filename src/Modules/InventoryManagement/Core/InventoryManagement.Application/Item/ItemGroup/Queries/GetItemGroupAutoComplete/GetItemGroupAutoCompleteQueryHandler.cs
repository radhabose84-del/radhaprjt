
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupAutoComplete
{
    public class GetItemGroupAutoCompleteQueryHandler : IRequestHandler<GetItemGroupAutoCompleteQuery,List<ItemGroupAutoCompleteDto>>
    {
        private readonly IItemGroupQueryRepository _itemGroupQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public GetItemGroupAutoCompleteQueryHandler(IItemGroupQueryRepository itemGroupQueryRepository, IMediator mediator, IMapper mapper)
        {
            _itemGroupQueryRepository = itemGroupQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<List<ItemGroupAutoCompleteDto>> Handle(GetItemGroupAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _itemGroupQueryRepository.GetItemGroupAutoCompleteAsync(request.SearchPattern ?? string.Empty);
            var itemGroup = _mapper.Map<List<ItemGroupAutoCompleteDto>>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetItemGroupAutoCompleteQueryHandler",        
                actionName: itemGroup.Count.ToString(),
                details: $"ItemGroup details was fetched.",
                module:"ItemGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return itemGroup;
        }
    }
}