
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Domain.Events;
using MediatR;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Application.ItemSpecificationValue.Commands.UpdateItemSpecificationValue
{
    public class UpdateItemSpecificationValueCommandHandler : IRequestHandler<UpdateItemSpecificationValueCommand, ApiResponseDTO<int>>
    {
        private readonly IItemSpecificationValueCommandRepository _commandRepository;
        private readonly IItemSpecificationValueQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateItemSpecificationValueCommandHandler(
            IItemSpecificationValueCommandRepository commandRepository,
            IItemSpecificationValueQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateItemSpecificationValueCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsItemSpecificationValueLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules("This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<DomainEntities.ItemSpecificationValue>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "ITEMSPECIFICATIONVALUE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Item Specification Value with Id {request.Id} updated successfully.",
                module: "ItemSpecificationValue"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Item Specification Value updated successfully.",
                Data = updatedId
            };
        }
    }
}
