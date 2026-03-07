namespace SalesManagement.Application.DeliveryChallan.Dto
{
    public class DeliveryChallanDetailDto
    {
        public int Id { get; set; }
        public int DeliveryChallanHeaderId { get; set; }

        // STO Detail Reference (same-module JOIN)
        public int StoDetailId { get; set; }

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

        // Quantity & UOM (cross-module lookup)
        public decimal DispatchQuantity { get; set; }
        public int UOMId { get; set; }
        public string? UOMName { get; set; }

        // Counts
        public int? BagCount { get; set; }
        public int? BaleCount { get; set; }

        // Weights
        public decimal NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }

        // Pricing
        public decimal ExMillRate { get; set; }
        public decimal LineMovementValue { get; set; }
    }
}
