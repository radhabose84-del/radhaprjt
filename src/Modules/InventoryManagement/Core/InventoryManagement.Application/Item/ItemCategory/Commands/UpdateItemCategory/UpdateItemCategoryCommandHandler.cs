
using AutoMapper;
using InventoryManagement.Application.Common.Exceptions;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace  InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory
{
    public class UpdateItemCategoryCommandHandler  : IRequestHandler<UpdateItemCategoryCommand, int>
    {
        private readonly IItemCategoryCommandRepository _itemCategoryCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public UpdateItemCategoryCommandHandler(IItemCategoryCommandRepository itemCategoryCommandRepository, IMediator mediator, IMapper mapper)
        {
            _itemCategoryCommandRepository = itemCategoryCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(UpdateItemCategoryCommand request, CancellationToken cancellationToken)
        {       
            var itemCategory = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(request);
            var result = await _itemCategoryCommandRepository.UpdateAsync(request.Id, itemCategory);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: itemCategory.Id.ToString(),
                actionName: itemCategory.ItemCategoryName ?? "",
                details: $"Notification Config was updated",
                module: "NotificationConfig");
            await _mediator.Publish(domainEvent, cancellationToken);
           
            return result > 0 ? result : throw new ExceptionRules("Notification Config update failed.");   
        }
    }
}