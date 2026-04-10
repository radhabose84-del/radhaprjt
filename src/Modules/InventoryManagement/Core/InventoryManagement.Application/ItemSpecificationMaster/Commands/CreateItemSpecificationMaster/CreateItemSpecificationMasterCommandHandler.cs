
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Domain.Events;
using MediatR;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Application.ItemSpecificationMaster.Commands.CreateItemSpecificationMaster
{
    public class CreateItemSpecificationMasterCommandHandler : IRequestHandler<CreateItemSpecificationMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IItemSpecificationMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateItemSpecificationMasterCommandHandler(
            IItemSpecificationMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateItemSpecificationMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<DomainEntities.ItemSpecificationMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "ITEMSPECIFICATIONMASTER_CREATE",
                actionName: request.SpecificationCode ?? string.Empty,
                details: $"Item Specification Master '{request.SpecificationCode}' created successfully with Id {newId}.",
                module: "ItemSpecificationMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Item Specification Master created successfully.",
                Data = newId
            };
        }
    }
}
