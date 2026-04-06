using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class DiscountPaymentTerm : BaseEntity
    {
        public int DiscountMasterId { get; set; }
        public int PaymentTermId { get; set; }

        // Navigation property (same-module only — PaymentTerm is cross-module, no nav prop)
        public DiscountMaster? DiscountMaster { get; set; }
    }
}
