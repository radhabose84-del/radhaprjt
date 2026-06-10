using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordAutoComplete
{
    public class GetVehicleMovementRecordAutoCompleteQueryHandler : IRequestHandler<GetVehicleMovementRecordAutoCompleteQuery, IReadOnlyList<VehicleMovementRecordAutoCompleteDto>>
    {
        private readonly IVehicleMovementRecordQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetVehicleMovementRecordAutoCompleteQueryHandler(IVehicleMovementRecordQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<VehicleMovementRecordAutoCompleteDto>> Handle(GetVehicleMovementRecordAutoCompleteQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, request.PurposeOfVisitId, cancellationToken);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetVehicleMovementRecordAutoCompleteQuery",
                actionName: result.Count.ToString(),
                details: "VehicleMovementRecord details was fetched.",
                module: "VehicleMovementRecord"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
