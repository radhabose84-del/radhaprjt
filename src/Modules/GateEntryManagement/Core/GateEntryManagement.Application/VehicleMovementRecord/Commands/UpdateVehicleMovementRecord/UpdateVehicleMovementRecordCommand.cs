using Contracts.Common;
using MediatR;

namespace GateEntryManagement.Application.VehicleMovementRecord.Commands.UpdateVehicleMovementRecord
{
    public class UpdateVehicleMovementRecordCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }

        // Vehicle Details (editable until Gate Out)
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

        // Additional
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
    }
}
