using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesItemPriceMaster.Commands.UpdateSalesItemPriceMaster
{
    public class UpdateSalesItemPriceMasterCommandHandler
        : IRequestHandler<UpdateSalesItemPriceMasterCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesItemPriceMasterCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateSalesItemPriceMasterCommandHandler(
            ISalesItemPriceMasterCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            UpdateSalesItemPriceMasterCommand request,
            CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesItemPriceMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_ITEM_PRICE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Item Price Master with Id {request.Id} updated successfully.",
                module: "SalesItemPriceMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Item Price Master updated successfully.",
                Data = updatedId
            };
        }
    }
}
