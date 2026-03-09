namespace SalesManagement.Application.StoReceipt.Dto
{
    public class StoReceiptDetailDto
    {
        public int Id { get; set; }
        public int StoReceiptHeaderId { get; set; }

        // DC Detail Reference (same-module JOIN)
        public int DeliveryChallanDetailId { get; set; }

        // Item (cross-module lookup)
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        // Lot (same-module JOIN)
        public int LotId { get; set; }
        public string? LotCode { get; set; }

        // Pack Numbers
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }

        // Quantities
        public decimal DispatchQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal DamageQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }

        // UOM (cross-module lookup)
        public int UOMId { get; set; }
        public string? UOMName { get; set; }

        // Counts
        public int? BagCount { get; set; }

        // Weights
        public decimal NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }

        // Line Status (same-module JOIN)
        public int LineStatusId { get; set; }
        public string? LineStatusName { get; set; }
    }
}
