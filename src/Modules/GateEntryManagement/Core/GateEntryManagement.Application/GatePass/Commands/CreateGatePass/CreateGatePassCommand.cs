using Contracts.Common;
using GateEntryManagement.Application.GatePass.Dto;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Commands.CreateGatePass
{
    public class CreateGatePassCommand : IRequest<ApiResponseDTO<int>>
    {
        public DateOnly GatePassDate { get; set; }
        public int VehicleMovementRecordId { get; set; }
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverMobile { get; set; }
        public string? TransporterName { get; set; }
        public int UnitId { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalDocumentQty { get; set; }
        public decimal TotalDispatchQty { get; set; }
        public int? ReturnableItems { get; set; }
        public decimal TotalValue { get; set; }

        // Weighbridge
        public decimal? GrossWeight { get; set; }
        public decimal? TareWeight { get; set; }
        public decimal? NetWeight { get; set; }

        public string? Remarks { get; set; }

        public List<CreateGatePassDetailDto>? GatePassDetails { get; set; }
    }
}
