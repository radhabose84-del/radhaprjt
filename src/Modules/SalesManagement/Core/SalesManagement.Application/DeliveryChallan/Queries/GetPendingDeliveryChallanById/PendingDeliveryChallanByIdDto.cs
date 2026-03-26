using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Queries.GetPendingDeliveryChallanById
{
    public class PendingDeliveryChallanByIdDto
    {
        public int Id { get; set; }
        public string? DeliveryNumber { get; set; }
        public DateOnly DeliveryDate { get; set; }

        // STO Reference
        public int StoHeaderId { get; set; }
        public string? StoNumber { get; set; }

        // From Plant & Storage Location
        public int FromPlantId { get; set; }
        public string? FromPlantName { get; set; }
        public int FromStorageLocationId { get; set; }
        public string? FromStorageLocationName { get; set; }

        // To Plant & Storage Location
        public int ToPlantId { get; set; }
        public string? ToPlantName { get; set; }
        public int ToStorageLocationId { get; set; }
        public string? ToStorageLocationName { get; set; }

        // Transporter
        public int TransporterId { get; set; }
        public string? TransporterName { get; set; }
        public string? VehicleNumber { get; set; }
        public decimal? TransportDistance { get; set; }

        // Values
        public decimal DeliveryValue { get; set; }
        public decimal ConsignmentValue { get; set; }

        // Status
        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        public string? Remarks { get; set; }

        // Workflow fields
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }

        // Detail lines
        public List<DeliveryChallanDetailDto>? DeliveryChallanDetails { get; set; }
    }
}
