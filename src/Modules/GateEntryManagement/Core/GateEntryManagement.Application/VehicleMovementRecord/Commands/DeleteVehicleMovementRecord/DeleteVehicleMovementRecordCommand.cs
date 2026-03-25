using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Commands.DeleteVehicleMovementRecord
{
    public sealed record DeleteVehicleMovementRecordCommand(int Id) : IRequest<bool>;
}
