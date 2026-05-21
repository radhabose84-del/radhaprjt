using Contracts.Common;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Commands.CreateVehicleMovementRecord
{
    public class CreateVehicleMovementRecordCommand : IRequest<ApiResponseDTO<int>>
    {
        // Vehicle Details
        public string? VehicleNumber { get; set; }
        public string? DriverName { get; set; }
        public string? DriverLicenseNo { get; set; }
        public string? DriverMobileNo { get; set; }
        public int? TransporterId { get; set; }

        // Basic Information
        public int PurposeOfVisitId { get; set; }
        public int? ReceivingTypeId { get; set; }
        public int? ReferenceDocTypeId { get; set; }
        public string? ReferenceDocNo { get; set; }

        // Cross-module
        public int UnitId { get; set; }

        // Additional
        public string? Remarks { get; set; }
    }
}
