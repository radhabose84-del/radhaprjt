
using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Domain.Events;
using MediatR;
using DomainEntities = InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Application.ItemSpecificationMaster.Commands.UpdateItemSpecificationMaster
{
    public class UpdateItemSpecificationMasterCommandHandler : IRequestHandler<UpdateItemSpecificationMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IItemSpecificationMasterCommandRepository _commandRepository;
        private readonly IItemSpecificationMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateItemSpecificationMasterCommandHandler(
            IItemSpecificationMasterCommandRepository commandRepository,
            IItemSpecificationMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateItemSpecificationMasterCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsItemSpecificationMasterLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules("This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<DomainEntities.ItemSpecificationMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "ITEMSPECIFICATIONMASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Item Specification Master with Id {request.Id} updated successfully.",
                module: "ItemSpecificationMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Item Specification Master updated successfully.",
                Data = updatedId
            };
        }
    }
}
