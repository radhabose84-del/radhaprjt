using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordById
{
    public class GetVehicleMovementRecordByIdQuery : IRequest<VehicleMovementRecordDto?>
    {
        public int Id { get; set; }
    }
}
