
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Domain.Events;
using MediatR;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Application.ItemSpecificationValue.Commands.CreateItemSpecificationValue
{
    public class CreateItemSpecificationValueCommandHandler : IRequestHandler<CreateItemSpecificationValueCommand, ApiResponseDTO<int>>
    {
        private readonly IItemSpecificationValueCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateItemSpecificationValueCommandHandler(
            IItemSpecificationValueCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateItemSpecificationValueCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<DomainEntities.ItemSpecificationValue>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "ITEMSPECIFICATIONVALUE_CREATE",
                actionName: request.SpecificationValue ?? string.Empty,
                details: $"Item Specification Value '{request.SpecificationValue}' created successfully with Id {newId}.",
                module: "ItemSpecificationValue"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Item Specification Value created successfully.",
                Data = newId
            };
        }
    }
}
