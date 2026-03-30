namespace SalesManagement.Application.DeliveryChallan.Dto
{
    public class DeliveryChallanHeaderDto
    {
        public int Id { get; set; }
        public string? DeliveryNumber { get; set; }
        public DateOnly DeliveryDate { get; set; }

        // STO Reference (same-module JOIN)
        public int StoHeaderId { get; set; }
        public string? StoNumber { get; set; }

        // From Plant & Storage Location (cross-module lookup)
        public int FromPlantId { get; set; }
        public string? FromPlantName { get; set; }
        public int FromStorageLocationId { get; set; }
        public string? FromStorageLocationName { get; set; }

        // To Plant & Storage Location (cross-module lookup)
        public int ToPlantId { get; set; }
        public string? ToPlantName { get; set; }
        public int ToStorageLocationId { get; set; }
        public string? ToStorageLocationName { get; set; }

        // Transporter (cross-module lookup)
        public int TransporterId { get; set; }
        public string? TransporterName { get; set; }
        public string? VehicleNumber { get; set; }
        public decimal? TransportDistance { get; set; }

        // Values
        public decimal DeliveryValue { get; set; }
        public decimal ConsignmentValue { get; set; }

        // Status (same-module JOIN)
        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        public string? Remarks { get; set; }

        // Workflow fields (populated only in GetPendingAsync, null/0 in GetAll)
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Audit fields
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }

        // Child details (populated only in GetById, null in GetAll)
        public List<DeliveryChallanDetailDto>? DeliveryChallanDetails { get; set; }
    }
}
