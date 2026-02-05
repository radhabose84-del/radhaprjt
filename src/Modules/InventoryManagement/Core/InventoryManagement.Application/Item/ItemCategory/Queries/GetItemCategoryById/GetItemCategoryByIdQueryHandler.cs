using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryById
{
    public class GetItemCategoryByIdQueryHandler : IRequestHandler<GetItemCategoryByIdQuery, ItemCategoryDto>
    {
        private readonly IItemCategoryQueryRepository _itemCategoryQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public GetItemCategoryByIdQueryHandler(IItemCategoryQueryRepository itemCategoryQueryRepository, IMediator mediator, IMapper mapper)
        {
            _itemCategoryQueryRepository = itemCategoryQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ItemCategoryDto> Handle(GetItemCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _itemCategoryQueryRepository.GetByIdAsync(request.Id);   
            if (result == null)
            {
                throw new KeyNotFoundException($"Item Category with Id {request.Id} not found.");
            }         
            var itemCategory = _mapper.Map<ItemCategoryDto>(result);
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetItemCategoryByIdQuery",
                actionName: itemCategory.Id.ToString(),
                details: $"Item Category{itemCategory.Id} was fetched.",
                module: "Item Category"
            );
            await _mediator.Publish(domainEvent, cancellationToken);
            return itemCategory;
        }
    }
}