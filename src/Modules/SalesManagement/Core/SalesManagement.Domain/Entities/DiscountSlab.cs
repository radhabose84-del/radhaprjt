using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class DiscountSlab : BaseEntity
    {
        public int DiscountMasterId { get; set; }
        public int SlabOrder { get; set; }
        public decimal FromValue { get; set; }
        public decimal? ToValue { get; set; }
        public decimal DiscountValue { get; set; }

        // Navigation property
        public DiscountMaster? DiscountMaster { get; set; }

        // Reverse navigation (SalesOrderDiscount)
        public ICollection<SalesOrderDiscount>? SalesOrderDiscounts { get; set; }
    }
}
