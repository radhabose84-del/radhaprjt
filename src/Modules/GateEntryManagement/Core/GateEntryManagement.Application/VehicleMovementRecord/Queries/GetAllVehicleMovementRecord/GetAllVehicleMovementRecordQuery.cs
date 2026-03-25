using Contracts.Common;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Queries.GetAllVehicleMovementRecord
{
    public class GetAllVehicleMovementRecordQuery : IRequest<ApiResponseDTO<List<VehicleMovementRecordDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
