using Contracts.Common;
using GateEntryManagement.Application.GateInward.Dto;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Commands.CreateGateInward
{
    public class CreateGateInwardCommand : IRequest<ApiResponseDTO<int>>
    {
        public int VehicleMovementRecordId { get; set; }
        public decimal? GrossWeight { get; set; }
        public decimal? TareWeight { get; set; }
        public bool QAInspectionRequired { get; set; }
        public int? QAStatusId { get; set; }
        public int UnitId { get; set; }
        public string? Remarks { get; set; }

        public List<CreateGateInwardDetailDto>? GateInwardDetails { get; set; }
    }
}
