using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordAutoComplete
{
    public sealed record GetVehicleMovementRecordAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<VehicleMovementRecordAutoCompleteDto>>;
}
