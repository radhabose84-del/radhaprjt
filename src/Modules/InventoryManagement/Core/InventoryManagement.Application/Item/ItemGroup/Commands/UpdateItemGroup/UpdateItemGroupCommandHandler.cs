
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup;
using InventoryManagement.Domain.Events;
using MediatR;

namespace  InventoryManagement.Application.Item.ItemGroup.Commands.UpdateItemGroup
{
    public class UpdateItemGroupCommandHandler  : IRequestHandler<UpdateItemGroupCommand, int>
    {
        private readonly IItemGroupCommandRepository _itemGroupCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        public UpdateItemGroupCommandHandler(IItemGroupCommandRepository itemGroupCommandRepository, IMediator mediator, IMapper mapper)
        {
            _itemGroupCommandRepository = itemGroupCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(UpdateItemGroupCommand request, CancellationToken cancellationToken)
        {       
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