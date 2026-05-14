using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory
{
    public class UpdateItemCategoryCommandHandler : IRequestHandler<UpdateItemCategoryCommand, int>
    {
        private readonly IItemCategoryCommandRepository _itemCategoryCommandRepository;
        private readonly IItemCategoryQueryRepository _itemCategoryQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateItemCategoryCommandHandler(IItemCategoryCommandRepository itemCategoryCommandRepository, IItemCategoryQueryRepository itemCategoryQueryRepository, IMediator mediator, IMapper mapper)
        {
            _itemCategoryCommandRepository = itemCategoryCommandRepository;
            _itemCategoryQueryRepository = itemCategoryQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(UpdateItemCategoryCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _itemCategoryQueryRepository.IsLinkedWithActiveItemsAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var itemCategory = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemCategory>(request);
            var unitConfigs = _mapper.Map<List<InventoryManagement.Domain.Entities.Item.ItemCategoryUnitConfig>>(request.SampleQuantities ?? new());

            var result = await _itemCategoryCommandRepository.UpdateAsync(request.Id, itemCategory, request.ModuleIds, unitConfigs);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: itemCategory.Id.ToString(),
                actionName: itemCategory.ItemCategoryName ?? "",
                details: $"Item Category was updated",
                module: "ItemCategory");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result > 0 ? result : throw new ExceptionRules("Item Category update failed.");
        }
    }
}
