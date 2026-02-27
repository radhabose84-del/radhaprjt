using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ItemPriceMaster.Commands.CreateItemPriceMaster
{
    public class CreateItemPriceMasterCommandHandler
        : IRequestHandler<CreateItemPriceMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IItemPriceMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateItemPriceMasterCommandHandler(
            IItemPriceMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateItemPriceMasterCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.ItemPriceMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "ITEM_PRICE_CREATE",
                actionName: request.PriceCode ?? string.Empty,
                details: $"Item Price Master '{request.PriceCode}' created successfully with Id {newId}.",
                module: "ItemPriceMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Item Price Master created successfully.",
                Data = newId
            };
        }
    }
}
