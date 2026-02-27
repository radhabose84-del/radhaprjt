using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster
{
    public class UpdateItemPriceMasterCommandHandler
        : IRequestHandler<UpdateItemPriceMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IItemPriceMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateItemPriceMasterCommandHandler(
            IItemPriceMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            UpdateItemPriceMasterCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ItemPriceMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "ITEM_PRICE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Item Price Master with Id {request.Id} updated successfully.",
                module: "ItemPriceMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Item Price Master updated successfully.",
                Data = updatedId
            };
        }
    }
}
