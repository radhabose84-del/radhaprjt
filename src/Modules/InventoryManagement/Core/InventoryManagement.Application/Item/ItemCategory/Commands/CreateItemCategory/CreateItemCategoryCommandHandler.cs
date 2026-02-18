using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Commands.CreateItemCategory
{
    public class CreateItemCategoryCommandHandler : IRequestHandler<CreateItemCategoryCommand, int>
    {
        private readonly IItemCategoryCommandRepository _itemCategoryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateItemCategoryCommandHandler(IItemCategoryCommandRepository itemCategoryRepository, IMediator mediator, IMapper mapper)
        {
            _itemCategoryRepository = itemCategoryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(CreateItemCategoryCommand request, CancellationToken cancellationToken)
        {
            var itemCategory = _mapper.Map<Domain.Entities.Item.ItemCategory>(request);
            var result = await _itemCategoryRepository.CreateAsync(itemCategory);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: itemCategory.Id.ToString(),
                actionName: itemCategory.ItemCategoryName ?? "",
                details: $"Item Category details was created",
                module: "itemCategory");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result > 0 ? result : throw new ExceptionRules("ItemCategory Creation Failed.");
        }
    }

}
