using AutoMapper;
using Contracts.Common;
using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IFreightMaster;
using LogisticsManagement.Domain.Events;

namespace LogisticsManagement.Application.FreightMaster.Commands.UpdateFreightMaster
{
    public class UpdateFreightMasterCommandHandler : IRequestHandler<UpdateFreightMasterCommand, ApiResponseDTO<int>>
    {
        private readonly IFreightMasterCommandRepository _commandRepository;
        private readonly IFreightMasterQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateFreightMasterCommandHandler(
            IFreightMasterCommandRepository commandRepository,
            IFreightMasterQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateFreightMasterCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.FreightMaster>(request);

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "FREIGHT_MASTER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"FreightMaster with Id {request.Id} updated successfully.",
                module: "FreightMaster"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "FreightMaster updated successfully.",
                Data = updatedId
            };
        }
    }
}
