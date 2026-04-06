using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class DiscountSalesGroup : BaseEntity
    {
        public int DiscountMasterId { get; set; }
        public int SalesGroupId { get; set; }

        // Navigation properties
        public DiscountMaster? DiscountMaster { get; set; }
        public SalesGroup? SalesGroup { get; set; }
    }
}
