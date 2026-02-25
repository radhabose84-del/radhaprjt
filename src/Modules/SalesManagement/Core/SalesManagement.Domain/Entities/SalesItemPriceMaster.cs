using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesItemPriceMaster : BaseEntity
    {
        public string PriceCode { get; set; } = null!;
        public int ItemId { get; set; }
        public int SalesSegmentId { get; set; }
        public int PaymentTermsId { get; set; }
        public decimal ExMillPrice { get; set; }
        public int CurrencyId { get; set; }
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset ValidTo { get; set; }

        // Navigation property (same-module FK)
        public SalesSegment SalesSegment { get; set; } = null!;
    }
}
