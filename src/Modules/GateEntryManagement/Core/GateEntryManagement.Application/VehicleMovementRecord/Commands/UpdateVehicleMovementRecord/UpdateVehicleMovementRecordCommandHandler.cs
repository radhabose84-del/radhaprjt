using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Commands.UpdateVehicleMovementRecord
{
    public class UpdateVehicleMovementRecordCommandHandler : IRequestHandler<UpdateVehicleMovementRecordCommand, ApiResponseDTO<int>>
    {
        private readonly IVehicleMovementRecordCommandRepository _commandRepository;
        private readonly IVehicleMovementRecordQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateVehicleMovementRecordCommandHandler(
            IVehicleMovementRecordCommandRepository commandRepository,
            IVehicleMovementRecordQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateVehicleMovementRecordCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.VehicleMovementRecord>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "VMR_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Vehicle Movement Record with Id {request.Id} updated successfully.",
                module: "VehicleMovementRecord"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Vehicle Movement Record updated successfully.",
                Data = result
            };
        }
    }
}
