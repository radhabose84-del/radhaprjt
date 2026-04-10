using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.PriceGroupMaster.Commands.CreatePriceGroupMaster
{
    public class CreatePriceGroupMasterCommandHandler : IRequestHandler<CreatePriceGroupMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IPriceGroupMasterCommandRepository _commandRepository;
        private readonly IPriceGroupMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreatePriceGroupMasterCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreatePriceGroupMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.PriceGroupMaster>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "PRICEGROUP_CREATE",
                actionName: request.PriceGroupCode,
                details: $"Price Group '{request.PriceGroupCode}' created successfully with Id {newId}.",
                module: "PriceGroupMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Price Group created successfully.",
                Data = newId
            };
        }
    }
}
