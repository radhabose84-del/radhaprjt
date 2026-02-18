using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Commands.DeleteItemCategory
{
    public class DeleteItemCategoryCommandHandler : IRequestHandler<DeleteItemCategoryCommand, int>
    {
        private readonly IItemCategoryCommandRepository _itemCategoryCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public DeleteItemCategoryCommandHandler(IItemCategoryCommandRepository itemCategoryCommandRepository, IMediator mediator, IMapper mapper)
        {
            _itemCategoryCommandRepository = itemCategoryCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(DeleteItemCategoryCommand request, CancellationToken cancellationToken)
        {            
            var itemCategory = _mapper.Map<Domain.Entities.Item.ItemCategory>(request);
            var result = await _itemCategoryCommandRepository.DeleteAsync(request.Id,itemCategory);          

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: itemCategory.Id.ToString(),
                actionName: itemCategory.ItemCategoryName ?? "",
                details: $"Item Category details was deleted",
                module: "Item Category  ");
            await _mediator.Publish(domainEvent);
            return result > 0 ? result : throw new ExceptionRules("Item Category was not found.");
        }


    }
}