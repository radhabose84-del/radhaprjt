using AutoMapper;
using InventoryManagement.Application.Common.Exceptions;
using InventoryManagement.Application.Common.Interfaces.Item.ItemGroup;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.Item.ItemGroup.Commands.DeleteItemGroup
{
    public class DeleteItemGroupCommandHandler : IRequestHandler<DeleteItemGroupCommand, int>
    {
        private readonly IItemGroupCommandRepository _itemGroupCommandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public DeleteItemGroupCommandHandler(IItemGroupCommandRepository itemGroupCommandRepository, IMediator mediator, IMapper mapper)
        {
            _itemGroupCommandRepository = itemGroupCommandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<int> Handle(DeleteItemGroupCommand request, CancellationToken cancellationToken)
        {            
            var itemGroup = _mapper.Map<Domain.Entities.Item.ItemGroup>(request);
            var result = await _itemGroupCommandRepository.DeleteAsync(request.Id,itemGroup);          

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: itemGroup.Id.ToString(),
                actionName: itemGroup.ItemGroupName ?? "",
                details: $"Item Group details was deleted",
                module: "Item Group  ");
            await _mediator.Publish(domainEvent);
            return result > 0 ? result : throw new ExceptionRules("Item Group was not found.");
        }


    }
}