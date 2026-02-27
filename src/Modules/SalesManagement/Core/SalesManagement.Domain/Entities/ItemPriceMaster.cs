using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ItemPriceMaster : BaseEntity
    {
        public string? PriceCode { get; set; }
        public int ItemId { get; set; }
        public int SalesSegmentId { get; set; }
        public int PaymentTermsId { get; set; }
        public decimal ExMillRate { get; set; }
        public int CurrencyId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }

        // Navigation property (same-module FK)
        public SalesSegment? SalesSegment { get; set; }
    }
}
