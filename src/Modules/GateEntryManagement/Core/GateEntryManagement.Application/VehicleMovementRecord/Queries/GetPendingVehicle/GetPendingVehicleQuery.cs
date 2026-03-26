using Contracts.Common;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Queries.GetPendingVehicle
{
    public class GetPendingVehicleQuery : IRequest<ApiResponseDTO<List<PendingVehicleDto>>>
    {
        public int UnitId { get; set; }
        public string? VehicleMovementId { get; set; }
        public string? VehicleNumber { get; set; }
    }
}
