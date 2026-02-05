using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroupById
{
    public class GetItemGroupByIdQueryHandler : IRequestHandler<GetItemGroupByIdQuery, ItemGroupDto>
    {
        private readonly IItemGroupQueryRepository _itemGroupQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetItemGroupByIdQueryHandler(IItemGroupQueryRepository itemGroupQueryRepository, IMediator mediator, IMapper mapper)
        {
            _itemGroupQueryRepository = itemGroupQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ItemGroupDto> Handle(GetItemGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _itemGroupQueryRepository.GetByIdAsync(request.Id);   
            if (result == null)
            {
                throw new KeyNotFoundException($"Item Group with Id {request.Id} not found.");
            }         
            var itemGroup = _mapper.Map<ItemGroupDto>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetItemGroupByIdQuery",
                actionName: itemGroup.Id.ToString(),
                details: $"Item Group{itemGroup.Id} was fetched.",
                module: "Item Group"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return itemGroup;
        }
    }
}