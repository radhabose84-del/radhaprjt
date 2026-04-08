using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ItemPriceMaster : BaseEntity
    {
        public string? PriceCode { get; set; }
        public int ItemId { get; set; }
        public int? VariantId { get; set; }               // Cross-module FK → Inventory.ItemMaster (variant)
        public int SalesSegmentId { get; set; }
        public decimal BaseRate { get; set; }
        public int CurrencyId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }
        public decimal? TolerancePercentage { get; set; }
        public decimal? CharityValue { get; set; }
        public decimal? HandlingCharges { get; set; }
        public int? StatusId { get; set; }                // FK → Sales.MiscMaster

        // Navigation properties (same-module FK)
        public SalesSegment? SalesSegment { get; set; }
        public MiscMaster? StatusMisc { get; set; }
    }
}
