using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Commands.UpdatePriceGroupMaster
{
    public class UpdatePriceGroupMasterCommandHandler : IRequestHandler<UpdatePriceGroupMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IPriceGroupMasterCommandRepository _commandRepository;
        private readonly IPriceGroupMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdatePriceGroupMasterCommandHandler(
            IPriceGroupMasterCommandRepository commandRepository,
            IPriceGroupMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdatePriceGroupMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.PriceGroupMaster>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "PRICEGROUP_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Price Group with Id {request.Id} updated successfully.",
                module: "PriceGroupMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Price Group updated successfully.",
                Data = result
            };
        }
    }
}
