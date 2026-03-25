using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordById
{
    public class GetVehicleMovementRecordByIdQueryHandler : IRequestHandler<GetVehicleMovementRecordByIdQuery, VehicleMovementRecordDto?>
    {
        private readonly IVehicleMovementRecordQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVehicleMovementRecordByIdQueryHandler(IVehicleMovementRecordQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<VehicleMovementRecordDto?> Handle(GetVehicleMovementRecordByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetVehicleMovementRecordByIdQuery",
                actionName: result.Id.ToString(),
                details: $"VehicleMovementRecord details {result.Id} was fetched.",
                module: "VehicleMovementRecord"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
