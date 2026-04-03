
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup
{
    public class UpdateItemGroupCommandHandler  : IRequestHandler<UpdateItemGroupCommand, int>
    {
        private readonly IItemGroupCommandRepository _itemGroupCommandRepository;
        private readonly IItemGroupQueryRepository _itemGroupQueryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public UpdateItemGroupCommandHandler(IItemGroupCommandRepository itemGroupCommandRepository, IItemGroupQueryRepository itemGroupQueryRepository, IMediator mediator, IMapper mapper)
        {
            _itemGroupCommandRepository = itemGroupCommandRepository;
            _itemGroupQueryRepository = itemGroupQueryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(UpdateItemGroupCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _itemGroupQueryRepository.IsItemGroupLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var itemGroup = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemGroup>(request);
            var result = await _itemGroupCommandRepository.UpdateAsync(request.Id, itemGroup);
            
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: itemGroup.Id.ToString(),
                actionName: itemGroup.ItemGroupName ?? "",
                details: $"Notification Config was updated",
                module: "NotificationConfig");
            await _mediator.Publish(domainEvent, cancellationToken);
           
            return result > 0 ? result : throw new ExceptionRules("Notification Config update failed.");   
        }
    }
}