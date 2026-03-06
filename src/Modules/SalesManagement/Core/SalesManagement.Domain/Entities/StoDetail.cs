namespace SalesManagement.Domain.Entities
{
    public class StoDetail
    {
        public int Id { get; set; }
        public int StoHeaderId { get; set; }
        public StoHeader StoHeader { get; set; } = null!;

        // Item Details (cross-module FK → InventoryManagement)
        public int ItemId { get; set; }

        // Quantity & UOM
        public decimal Quantity { get; set; }
        public int UOMId { get; set; }

        // Pricing
        public decimal TransferPrice { get; set; }

        // Line Status (same-module FK → Sales.MiscMaster, MiscType = "StoLineItemStatus")
        public int? LineStatusId { get; set; }
        public MiscMaster? LineStatus { get; set; }
    }
}
