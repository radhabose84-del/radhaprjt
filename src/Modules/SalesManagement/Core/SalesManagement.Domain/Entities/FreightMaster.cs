using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class FreightMaster : BaseEntity
    {
        public int FreightModeId { get; set; }
        public int RateMethodId { get; set; }
        public decimal Rate { get; set; }

        // Navigation properties (same-module FK to MiscMaster)
        public MiscMaster? FreightMode { get; set; }
        public MiscMaster? RateMethod { get; set; }
    }
}
