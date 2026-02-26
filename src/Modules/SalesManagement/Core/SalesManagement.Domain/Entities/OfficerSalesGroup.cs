using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class OfficerSalesGroup : BaseEntity
    {
        public int MarketingOfficerId { get; set; }
        public int SalesGroupId { get; set; }

        // Navigation properties
        public MarketingOfficer MarketingOfficer { get; set; } = null!;
        public SalesGroup SalesGroup { get; set; } = null!;
    }
}
