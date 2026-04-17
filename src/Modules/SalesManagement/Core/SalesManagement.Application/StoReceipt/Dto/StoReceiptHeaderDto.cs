namespace SalesManagement.Application.StoReceipt.Dto
{
    public class StoReceiptHeaderDto
    {
        public int Id { get; set; }
        public string? StoReceiptNumber { get; set; }
        public DateOnly StoReceiptDate { get; set; }

        // Delivery Challan Reference (same-module JOIN)
        public int DeliveryChallanHeaderId { get; set; }
        public string? DeliveryNumber { get; set; }

        // Source Unit (dispatch plant — from Delivery Challan)
        public int? SourceUnitId { get; set; }
        public string? SourceUnitName { get; set; }

        // Receiving Plant (cross-module lookup)
        public int ReceivingPlantId { get; set; }
        public string? ReceivingPlantName { get; set; }

        // Receiving Storage Location (cross-module lookup)
        public int ReceivingStorageLocationId { get; set; }
        public string? ReceivingStorageLocationName { get; set; }

        // Bin / Rack (cross-module lookup)
        public int? BinId { get; set; }
        public string? BinName { get; set; }

        public string? VehicleNumber { get; set; }
        public string? Remarks { get; set; }

        // Status (same-module JOIN)
        public int StatusId { get; set; }
        public string? StatusName { get; set; }

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
        public List<StoReceiptDetailDto>? StoReceiptDetails { get; set; }
    }
}
