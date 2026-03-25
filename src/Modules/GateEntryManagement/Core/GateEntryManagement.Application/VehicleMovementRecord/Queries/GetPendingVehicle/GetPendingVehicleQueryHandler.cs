using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Queries.GetPendingVehicle
{
    public class GetPendingVehicleQueryHandler : IRequestHandler<GetPendingVehicleQuery, ApiResponseDTO<List<PendingVehicleDto>>>
    {
        private readonly IVehicleMovementRecordQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPendingVehicleQueryHandler(IVehicleMovementRecordQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<PendingVehicleDto>>> Handle(GetPendingVehicleQuery request, CancellationToken cancellationToken)
        {
            var data = await _queryRepository.GetPendingVehiclesAsync(
                request.UnitId, request.VehicleMovementId, request.VehicleNumber, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetPendingVehicleQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Pending vehicle details were fetched.",
                module: "VehicleMovementRecord"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<PendingVehicleDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = data,
                TotalCount = data.Count
            };
        }
    }
}
