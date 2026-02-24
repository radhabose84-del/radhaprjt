#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesItemPriceMaster.Commands.CreateSalesItemPriceMaster
{
    public class CreateSalesItemPriceMasterCommandHandler
        : IRequestHandler<CreateSalesItemPriceMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesItemPriceMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateSalesItemPriceMasterCommandHandler(
            ISalesItemPriceMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateSalesItemPriceMasterCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesItemPriceMaster>(request);
            entity.IsActive = Domain.Common.BaseEntity.Status.Active;
            entity.IsDeleted = Domain.Common.BaseEntity.IsDelete.NotDeleted;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_ITEM_PRICE_CREATE",
                actionName: request.PriceCode,
                details: $"Sales Item Price Master '{request.PriceCode}' created successfully with Id {newId}.",
                module: "SalesItemPriceMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Item Price Master created successfully.",
                Data = newId
            };
        }
    }
}
