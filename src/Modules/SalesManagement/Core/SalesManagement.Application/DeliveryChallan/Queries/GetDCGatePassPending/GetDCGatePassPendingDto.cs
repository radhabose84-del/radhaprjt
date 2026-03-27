namespace SalesManagement.Application.DeliveryChallan.Queries.GetDCGatePassPending
{
    public class GetDCGatePassPendingDto
    {
        // Header fields (same as DeliveryChallanHeaderDto from GetById)
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
        public bool GEFlag { get; set; }
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

        // Detail lines
        public List<GetDCGatePassPendingDetailDto> DeliveryChallanDetails { get; set; } = new();

        public class GetDCGatePassPendingDetailDto
        {
            public int DCHeaderId { get; set; }
            public int StoDetailId { get; set; }
            public int ItemId { get; set; }
            public string? ItemCode { get; set; }
            public string? ItemName { get; set; }
            public int LotId { get; set; }
            public string? LotCode { get; set; }
            public int StartPackNo { get; set; }
            public int EndPackNo { get; set; }
            public decimal DispatchQuantity { get; set; }
            public int UOMId { get; set; }
            public string? UOMName { get; set; }
            public int? BagCount { get; set; }
            public int? BaleCount { get; set; }
            public decimal NetWeight { get; set; }
            public decimal? GrossWeight { get; set; }
            public decimal ExMillRate { get; set; }
            public decimal LineMovementValue { get; set; }
        }
    }
}
