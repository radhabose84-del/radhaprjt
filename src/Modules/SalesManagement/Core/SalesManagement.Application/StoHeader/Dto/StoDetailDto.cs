namespace SalesManagement.Application.StoHeader.Dto
{
    public class StoDetailDto
    {
        public int Id { get; set; }
        public int StoHeaderId { get; set; }

        // Item (cross-module lookup)
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        // Quantity & UOM (cross-module lookup)
        public decimal Quantity { get; set; }
        public int UOMId { get; set; }
        public string? UOMName { get; set; }

        // Pricing
        public decimal TransferPrice { get; set; }

        // Line Status (same-module JOIN)
        public int? LineStatusId { get; set; }
        public string? LineStatusName { get; set; }
    }
}
